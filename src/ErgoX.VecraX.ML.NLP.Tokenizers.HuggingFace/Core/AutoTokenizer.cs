using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

public sealed class AutoTokenizer : IDisposable
{
    private AutoTokenizer(
        Tokenizer tokenizer,
        TokenizerConfig? tokenizerConfig,
        SpecialTokensMap? specialTokens,
        string basePath,
        AutoTokenizerLoadOptions options)
    {
        Tokenizer = tokenizer;
        TokenizerConfig = tokenizerConfig;
        SpecialTokens = specialTokens;
        BasePath = basePath;
        Options = options;
    }

    public Tokenizer Tokenizer { get; }

    public TokenizerConfig? TokenizerConfig { get; }

    public SpecialTokensMap? SpecialTokens { get; }

    public string BasePath { get; }

    public AutoTokenizerLoadOptions Options { get; }

    public bool SupportsChatTemplate => !string.IsNullOrEmpty(TokenizerConfig?.ChatTemplate);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Factory method returns the disposable instance to the caller.")]
    public static AutoTokenizer Load(string location, AutoTokenizerLoadOptions? options = null)
    {
        var tokenizer = LoadAsync(location, options, CancellationToken.None).GetAwaiter().GetResult();
        return tokenizer;
    }

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

            if (resolvedOptions.ApplyTokenizerDefaults)
            {
                ApplyTokenizerDefaults(tokenizer, tokenizerConfig);
            }

            return new AutoTokenizer(tokenizer, tokenizerConfig, specialTokens, baseDirectory, resolvedOptions);
        }
        catch
        {
            tokenizer.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        Tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }

    public string ApplyChatTemplate(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedOptions = options ?? new ChatTemplateOptions();
        return RenderChatTemplate(resolvedMessages, resolvedOptions);
    }

    public EncodingResult ApplyChatTemplateAsEncoding(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
    {
        var resolvedMessages = MaterializeMessages(messages);
        var resolvedOptions = options ?? new ChatTemplateOptions();
        var prompt = RenderChatTemplate(resolvedMessages, resolvedOptions);
        return Tokenizer.Encode(prompt, addSpecialTokens: false);
    }

    public IReadOnlyList<int> ApplyChatTemplateAsTokenIds(IEnumerable<ChatMessage> messages, ChatTemplateOptions? options = null)
        => ApplyChatTemplateAsEncoding(messages, options).Ids;

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
}

public sealed class AutoTokenizerLoadOptions
{
    public bool ApplyTokenizerDefaults { get; set; } = true;
}
