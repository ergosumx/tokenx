using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
}

public sealed class AutoTokenizerLoadOptions
{
    public bool ApplyTokenizerDefaults { get; set; } = true;
}
