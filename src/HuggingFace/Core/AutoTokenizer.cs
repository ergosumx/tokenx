using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// Loads and manages HuggingFace tokenizer configurations, supporting encoding, decoding, chat templates, and generation defaults.
/// </summary>
/// <remarks>
/// This class provides a unified interface to load tokenizer.json alongside optional configuration files
/// (tokenizer_config.json, special_tokens_map.json, generation_config.json) from HuggingFace model directories.
/// It handles padding, truncation, chat templates, and generation settings automatically.
/// </remarks>
public sealed class AutoTokenizer : IDisposable
{
    private AutoTokenizer(
        Tokenizer tokenizer,
        TokenizerConfig? tokenizerConfig,
        SpecialTokensMap? specialTokens,
        GenerationConfig? generationConfig,
        string basePath,
        AutoTokenizerLoadOptions options)
    {
        Tokenizer = tokenizer;
        TokenizerConfig = tokenizerConfig;
        SpecialTokens = specialTokens;
        GenerationConfig = generationConfig;
        BasePath = basePath;
        Options = options;
    }

    /// <summary>
    /// Gets the underlying tokenizer instance for encoding and decoding operations.
    /// </summary>
    public Tokenizer Tokenizer { get; }

    /// <summary>
    /// Gets the tokenizer configuration, including padding, truncation, and chat template settings.
    /// May be null if tokenizer_config.json was not found or is empty.
    /// </summary>
    public TokenizerConfig? TokenizerConfig { get; }

    /// <summary>
    /// Gets the special tokens map defining tokens like [CLS], [SEP], [PAD], etc.
    /// May be null if special_tokens_map.json was not found or is empty.
    /// </summary>
    public SpecialTokensMap? SpecialTokens { get; }

    /// <summary>
    /// Gets the generation configuration with model-specific defaults (e.g., max_length, top_k, temperature).
    /// May be null if generation_config.json was not found or <see cref="AutoTokenizerLoadOptions.LoadGenerationConfig"/> was false.
    /// </summary>
    public GenerationConfig? GenerationConfig { get; }

    /// <summary>
    /// Gets the base directory path from which tokenizer files were loaded.
    /// </summary>
    public string BasePath { get; }

    /// <summary>
    /// Gets the load options that were applied when this tokenizer was instantiated.
    /// </summary>
    public AutoTokenizerLoadOptions Options { get; }

    /// <summary>
    /// Gets a value indicating whether this tokenizer supports chat template rendering.
    /// </summary>
    public bool SupportsChatTemplate => !string.IsNullOrEmpty(TokenizerConfig?.ChatTemplate);

    /// <summary>
    /// Gets a value indicating whether this tokenizer provides generation defaults.
    /// </summary>
    public bool SupportsGenerationDefaults => GenerationConfig is not null;

    /// <summary>
    /// Loads a tokenizer synchronously from a file or directory path.
    /// </summary>
    /// <param name="location">
    /// Path to a tokenizer.json file or a directory containing it.
    /// Optional configuration files (tokenizer_config.json, special_tokens_map.json, generation_config.json)
    /// will be loaded if present.
    /// </param>
    /// <param name="options">Load options controlling whether to apply defaults and load generation config.
    /// Defaults to applying tokenizer defaults and loading generation config.</param>
    /// <returns>An initialized <see cref="AutoTokenizer"/> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the tokenizer.json file is not found.</exception>
    /// <example>
    /// <code>
    /// // Load from a directory
    /// using var tokenizer = AutoTokenizer.Load("path/to/model/directory");
    ///
    /// // Load with custom options
    /// var options = new AutoTokenizerLoadOptions { ApplyTokenizerDefaults = false };
    /// using var tokenizer = AutoTokenizer.Load("tokenizer.json", options);
    /// </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Factory method returns the disposable instance to the caller.")]
    public static AutoTokenizer Load(string location, AutoTokenizerLoadOptions? options = null)
    {
        var tokenizer = LoadAsync(location, options, CancellationToken.None).GetAwaiter().GetResult();
        return tokenizer;
    }

    /// <summary>
    /// Loads a tokenizer asynchronously from a file or directory path.
    /// </summary>
    /// <param name="location">
    /// Path to a tokenizer.json file or a directory containing it.
    /// Optional configuration files (tokenizer_config.json, special_tokens_map.json, generation_config.json)
    /// will be loaded if present.
    /// </param>
    /// <param name="options">Load options controlling whether to apply defaults and load generation config.
    /// Defaults to applying tokenizer defaults and loading generation config.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task that resolves to an initialized <see cref="AutoTokenizer"/> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the tokenizer.json file is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if configuration files are malformed.</exception>
    /// <example>
    /// <code>
    /// using var tokenizer = await AutoTokenizer.LoadAsync("model/directory", cancellationToken: ct);
    /// </code>
    /// </example>
    public static async Task<AutoTokenizer> LoadAsync(string location, AutoTokenizerLoadOptions? options = null, CancellationToken cancellationToken = default)
    {
        var resolvedOptions = options ?? new AutoTokenizerLoadOptions();
        var fullPath = Path.GetFullPath(location);

        string baseDirectory;
        string tokenizerPath;

        if (File.Exists(fullPath))
        {
            baseDirectory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
            tokenizerPath = fullPath;
        }
        else if (Directory.Exists(fullPath))
        {
            baseDirectory = fullPath;
            tokenizerPath = Path.Combine(baseDirectory, "tokenizer.json");
        }
        else
        {
            throw new FileNotFoundException("Tokenizer path could not be resolved.", fullPath);
        }

        if (!File.Exists(tokenizerPath))
        {
            throw new FileNotFoundException("tokenizer.json file not found in the provided location.", tokenizerPath);
        }

        var tokenizer = Tokenizer.FromFile(tokenizerPath);
        try
        {
            var tokenizerConfig = await TryLoadTokenizerConfigAsync(baseDirectory, cancellationToken).ConfigureAwait(false);
            var specialTokens = await TryLoadSpecialTokensAsync(baseDirectory, cancellationToken).ConfigureAwait(false);
            GenerationConfig? generationConfig = null;

            if (resolvedOptions.LoadGenerationConfig)
            {
                generationConfig = await TryLoadGenerationConfigAsync(baseDirectory, cancellationToken).ConfigureAwait(false);
            }

            if (resolvedOptions.ApplyTokenizerDefaults)
            {
                ApplyTokenizerDefaults(tokenizer, tokenizerConfig);
            }

            return new AutoTokenizer(tokenizer, tokenizerConfig, specialTokens, generationConfig, baseDirectory, resolvedOptions);
        }
        catch
        {
            tokenizer.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Releases all resources associated with this tokenizer.
    /// </summary>
    public void Dispose()
    {
        Tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Applies a chat template to format messages into a single prompt string.
    /// </summary>
    /// <param name="messages">The chat messages to format.</param>
    /// <param name="options">Optional chat template options (template override, add generation prompt, variables).</param>
    /// <returns>The rendered prompt string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the tokenizer does not support chat templates.</exception>
    /// <example>
    /// <code>
    /// var messages = new[]
    /// {
    ///     new ChatMessage { Role = "user", Content = "Hello" },
    ///     new ChatMessage { Role = "assistant", Content = "Hi there!" }
    /// };
    /// string prompt = tokenizer.ApplyChatTemplate(messages);
    /// </code>
    /// </example>
    public string ApplyChatTemplate(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedOptions = options ?? new ChatTemplateOptions();
        return RenderChatTemplate(resolvedMessages, resolvedOptions);
    }

    /// <summary>
    /// Applies a chat template and returns the result as an <see cref="EncodingResult"/> with token IDs.
    /// </summary>
    /// <param name="messages">The chat messages to format and encode.</param>
    /// <param name="options">Optional chat template options.</param>
    /// <returns>An <see cref="EncodingResult"/> containing token IDs for the formatted prompt.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the tokenizer does not support chat templates.</exception>
    public EncodingResult ApplyChatTemplateAsEncoding(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedOptions = options ?? new ChatTemplateOptions();
        var prompt = RenderChatTemplate(resolvedMessages, resolvedOptions);
        return Tokenizer.Encode(prompt, addSpecialTokens: false);
    }

    /// <summary>
    /// Applies a chat template and returns only the token IDs.
    /// </summary>
    /// <param name="messages">The chat messages to format and encode.</param>
    /// <param name="options">Optional chat template options.</param>
    /// <returns>A list of token IDs.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the tokenizer does not support chat templates.</exception>
    public IReadOnlyList<int> ApplyChatTemplateAsTokenIds(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
        => ApplyChatTemplateAsEncoding(messages, options).Ids;

    /// <summary>
    /// Initiates a generation request from a plain text prompt using generation settings.
    /// </summary>
    /// <param name="prompt">The input prompt text.</param>
    /// <param name="generationOptions">Optional generation options (overrides generation config defaults).</param>
    /// <returns>A <see cref="GenerationRequest"/> configured for text generation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if prompt is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if generation config is not available.</exception>
    /// <example>
    /// <code>
    /// var request = tokenizer.Generate("Once upon a time", new GenerationOptions { MaxLength = 100 });
    /// </code>
    /// </example>
    public GenerationRequest Generate(string prompt, GenerationOptions? generationOptions = null)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        var settings = ResolveGenerationSettings(generationOptions);
        return new GenerationRequest(prompt, settings, null);
    }

    /// <summary>
    /// Initiates a generation request from chat messages using generation settings.
    /// </summary>
    /// <param name="messages">The chat messages to format as a prompt.</param>
    /// <param name="chatOptions">Optional chat template options.</param>
    /// <param name="generationOptions">Optional generation options.</param>
    /// <returns>A <see cref="GenerationRequest"/> configured for generation from a chat prompt.</returns>
    /// <exception cref="InvalidOperationException">Thrown if chat templates or generation config are not supported.</exception>
    public GenerationRequest Generate(
        IEnumerable<ChatMessage> messages,
        ChatTemplateOptions? chatOptions = null,
        GenerationOptions? generationOptions = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedChatOptions = chatOptions ?? new ChatTemplateOptions();
        var prompt = RenderChatTemplate(resolvedMessages, resolvedChatOptions);
        var settings = ResolveGenerationSettings(generationOptions);
        return new GenerationRequest(prompt, settings, resolvedMessages);
    }

    /// <summary>
    /// Initiates a streaming generation request from a plain text prompt.
    /// </summary>
    /// <param name="prompt">The input prompt text.</param>
    /// <param name="generationOptions">Optional generation options.</param>
    /// <param name="streamOptions">Optional streaming-specific options.</param>
    /// <returns>A <see cref="StreamingGenerationRequest"/> for token-by-token generation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if prompt is null.</exception>
    public StreamingGenerationRequest GenerateStream(
        string prompt,
        GenerationOptions? generationOptions = null,
        StreamGenerationOptions? streamOptions = null)
    {
        if (prompt is null)
        {
            throw new ArgumentNullException(nameof(prompt));
        }

        var settings = ResolveGenerationSettings(generationOptions);
        var skipSpecialTokens = ResolveSkipSpecialTokens(streamOptions, settings);
        return new StreamingGenerationRequest(prompt, settings, null, skipSpecialTokens);
    }

    /// <summary>
    /// Initiates a streaming generation request from chat messages.
    /// </summary>
    /// <param name="messages">The chat messages to format as a prompt.</param>
    /// <param name="chatOptions">Optional chat template options.</param>
    /// <param name="generationOptions">Optional generation options.</param>
    /// <param name="streamOptions">Optional streaming-specific options.</param>
    /// <returns>A <see cref="StreamingGenerationRequest"/> for token-by-token generation from a chat prompt.</returns>
    public StreamingGenerationRequest GenerateStream(
        IEnumerable<ChatMessage> messages,
        ChatTemplateOptions? chatOptions = null,
        GenerationOptions? generationOptions = null,
        StreamGenerationOptions? streamOptions = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedChatOptions = chatOptions ?? new ChatTemplateOptions();
        var prompt = RenderChatTemplate(resolvedMessages, resolvedChatOptions);
        var settings = ResolveGenerationSettings(generationOptions);
        var skipSpecialTokens = ResolveSkipSpecialTokens(streamOptions, settings);
        return new StreamingGenerationRequest(prompt, settings, resolvedMessages, skipSpecialTokens);
    }

    private static async Task<TokenizerConfig?> TryLoadTokenizerConfigAsync(string baseDirectory, CancellationToken cancellationToken)
    {
        var path = Path.Combine(baseDirectory, "tokenizer_config.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(bytes);
        return TokenizerConfig.FromJson(json);
    }

    private static async Task<SpecialTokensMap?> TryLoadSpecialTokensAsync(string baseDirectory, CancellationToken cancellationToken)
    {
        var path = Path.Combine(baseDirectory, "special_tokens_map.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(bytes);
        return SpecialTokensMap.FromJson(json);
    }

    private static async Task<GenerationConfig?> TryLoadGenerationConfigAsync(string baseDirectory, CancellationToken cancellationToken)
    {
        var path = Path.Combine(baseDirectory, "generation_config.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(bytes);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        string normalized;
        try
        {
            normalized = Tokenizer.NormalizeGenerationConfig(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to normalize generation configuration at '{path}'.", ex);
        }

        try
        {
            return GenerationConfig.FromJson(normalized);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse generation configuration at '{path}'.", ex);
        }
    }

    private static void ApplyTokenizerDefaults(Tokenizer tokenizer, TokenizerConfig? config)
    {
        if (config is null)
        {
            return;
        }

        if (config.Padding is { } padding)
        {
            try
            {
                var direction = string.Equals(padding.Direction, "left", StringComparison.OrdinalIgnoreCase)
                    ? PaddingDirection.Left
                    : PaddingDirection.Right;

                var padToken = string.IsNullOrEmpty(padding.PadToken) ? "[PAD]" : padding.PadToken;
                var options = new PaddingOptions(direction, padding.PadId, padding.PadTypeId, padToken, padding.Length, padding.PadToMultipleOf);
                tokenizer.EnablePadding(options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to apply padding configuration to tokenizer.", ex);
            }
        }

        if (config.Truncation is { } truncation)
        {
            try
            {
                var strategy = truncation.Strategy switch
                {
                    "only_first" => TruncationStrategy.OnlyFirst,
                    "only_second" => TruncationStrategy.OnlySecond,
                    _ => TruncationStrategy.LongestFirst
                };

                var direction = string.Equals(truncation.Direction, "left", StringComparison.OrdinalIgnoreCase)
                    ? TruncationDirection.Left
                    : TruncationDirection.Right;

                var options = new TruncationOptions(truncation.MaxLength, truncation.Stride, strategy, direction);
                tokenizer.EnableTruncation(options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to apply truncation configuration to tokenizer.", ex);
            }
        }
    }

    private static IReadOnlyList<ChatMessage> MaterializeMessages(IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var resolved = messages as ChatMessage[] ?? messages.ToArray();
        if (resolved.Length == 0)
        {
            throw new ArgumentException("At least one chat message is required.", nameof(messages));
        }

        for (var index = 0; index < resolved.Length; index++)
        {
            if (resolved[index] is null)
            {
                throw new ArgumentException("Chat message collections cannot contain null entries.", nameof(messages));
            }
        }

        return resolved;
    }

    private string RenderChatTemplate(IReadOnlyList<ChatMessage> messages, ChatTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(options);

        var template = ResolveTemplate(options);
        var messagesJson = ChatMessageSerializer.Serialize(messages);
        var variablesSnapshot = BuildVariablesSnapshot(options);
        var variablesJson = ChatTemplatePayloadBuilder.BuildVariablesJson(TokenizerConfig, SpecialTokens, variablesSnapshot);

        return Tokenizer.ApplyChatTemplate(template, messagesJson, variablesJson, options.AddGenerationPrompt);
    }

    private string ResolveTemplate(ChatTemplateOptions options)
    {
        var candidate = options.TemplateOverride ?? TokenizerConfig?.ChatTemplate;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            throw new InvalidOperationException("The tokenizer configuration does not define a chat template.");
        }

        return candidate;
    }

    private GenerationSettings ResolveGenerationSettings(GenerationOptions? generationOptions)
    {
        if (GenerationConfig is null)
        {
            throw new InvalidOperationException("The tokenizer does not provide generation defaults.");
        }

        return GenerationConfig.BuildSettings(generationOptions);
    }

    private static IReadOnlyDictionary<string, JsonNode?>? BuildVariablesSnapshot(ChatTemplateOptions options)
    {
        if (options.AdditionalVariables.Count == 0)
        {
            return null;
        }

        var snapshot = new Dictionary<string, JsonNode?>(options.AdditionalVariables.Count, StringComparer.Ordinal);
        foreach (var pair in options.AdditionalVariables)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            snapshot[pair.Key] = pair.Value?.DeepClone();
        }

        return snapshot;
    }

    private static bool ResolveSkipSpecialTokens(StreamGenerationOptions? streamOptions, GenerationSettings settings)
    {
        if (streamOptions?.SkipSpecialTokens is bool explicitValue)
        {
            return explicitValue;
        }

        if (settings.SkipSpecialTokens is bool configured)
        {
            return configured;
        }

        if (settings.TryGetRawParameter("skip_special_tokens", out var rawNode) && rawNode is not null)
        {
            if (TryConvertToBoolean(rawNode, out var parsed))
            {
                return parsed;
            }
        }

        return true;
    }

    private static bool TryConvertToBoolean(JsonNode node, out bool value)
    {
        value = false;

        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<bool>(out var direct))
            {
                value = direct;
                return true;
            }

            if (jsonValue.TryGetValue<string>(out var text) && bool.TryParse(text, out var parsed))
            {
                value = parsed;
                return true;
            }

            if (jsonValue.TryGetValue<double>(out var numeric))
            {
                if (!double.IsNaN(numeric) && !double.IsInfinity(numeric))
                {
                    value = Math.Abs(numeric) >= double.Epsilon;
                    return true;
                }
            }
        }

        return false;
    }
}

public sealed class AutoTokenizerLoadOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically apply tokenizer configuration
    /// defaults for padding, truncation, and normalization.
    /// </summary>
    /// <remarks>
    /// When enabled (default), settings from tokenizer_config.json are applied to the underlying
    /// tokenizer, ensuring consistent preprocessing behavior. Set to false to use only the
    /// tokenizer.json model without configuration overrides.
    /// </remarks>
    public bool ApplyTokenizerDefaults { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to load generation_config.json for default
    /// generation parameters (e.g., max_length, top_k, temperature).
    /// </summary>
    /// <remarks>
    /// When enabled (default), generation settings are loaded and available via
    /// <see cref="AutoTokenizer.GenerationConfig"/>. Required for generation request
    /// and streaming generation methods.
    /// </remarks>
    public bool LoadGenerationConfig { get; set; } = true;
}
