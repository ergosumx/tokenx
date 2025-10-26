namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;
using Xunit.Sdk;
using Xunit.Abstractions;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class TokenizationTemplatesIntegrationTests : HuggingFaceTestBase
{
    private const string TargetIdentifier = "huggingface";
    private const string ModelFilterEnvironmentVariable = "TOKENX_TOKENIZATION_MODELS";

    private static readonly ConcurrentDictionary<string, TokenizationValidationManifest> ManifestCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<TokenizationTemplateCase> ContractCases = TokenizationTemplateCase.LoadAllForTarget(TargetIdentifier);

    private static readonly IReadOnlyList<string> ModelIdentifiers = ResolveModelIdentifiers();

    private static readonly JsonSerializerOptions SummarySerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly ITestOutputHelper _output;

    public TokenizationTemplatesIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static IEnumerable<object[]> TestMatrix()
    {
        foreach (var model in ModelIdentifiers)
        {
            foreach (var templateCase in ContractCases)
            {
                yield return new object[] { model, templateCase };
            }
        }
    }

    [Theory(DisplayName = "{1.Id} - {0}")]
    [MemberData(nameof(TestMatrix))]
    public void Model_encodes_tokenization_template_sequences(string modelFolder, TokenizationTemplateCase templateCase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelFolder);
        ArgumentNullException.ThrowIfNull(templateCase);

        var tokenizerPath = TestDataPath.GetModelTokenizerPath(modelFolder);
        Assert.True(File.Exists(tokenizerPath), $"Tokenizer asset missing at '{tokenizerPath}'. Run generate_benchmarks.py.");

        using var tokenizer = Tokenizer.FromFile(tokenizerPath);
        var encoding = templateCase.PairText is not null
            ? tokenizer.Encode(templateCase.Text, templateCase.PairText)
            : tokenizer.Encode(templateCase.Text);

        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0, $"Encoding produced zero tokens for template '{templateCase.Id}'.");

        var decoded = tokenizer.Decode(encoding.Ids);
        Assert.NotNull(decoded);

        var snapshot = CreateSnapshot(templateCase.Text, encoding);
        var manifest = ManifestCache.GetOrAdd(modelFolder, static model => TokenizationValidationManifest.Load(model));

        if (!manifest.TryGetCase(templateCase.Id, out var manifestCase) || manifestCase?.Python is null)
        {
            LogMissingBaseline(modelFolder, templateCase, snapshot);
            throw new XunitException($"python baseline missing for model '{modelFolder}' case '{templateCase.Id}'.");
        }

        var expected = manifestCase.Python;
        if (string.IsNullOrWhiteSpace(expected.TextHash) || string.IsNullOrWhiteSpace(expected.EncodingHash))
        {
            LogMissingBaseline(modelFolder, templateCase, snapshot);
            throw new XunitException($"python baseline incomplete for model '{modelFolder}' case '{templateCase.Id}'.");
        }

        Assert.Equal(expected.TextHash, snapshot.TextHash);
        Assert.Equal(expected.EncodingHash, snapshot.EncodingHash);
    }

    private void LogMissingBaseline(string modelFolder, TokenizationTemplateCase templateCase, TokenizationValidationSnapshot snapshot)
    {
        var payload = new
        {
            model = modelFolder,
            template = templateCase.Id,
            description = templateCase.Description,
            snapshot,
        };

        var json = JsonSerializer.Serialize(payload, SummarySerializerOptions);
        _output.WriteLine(json);
    }

    private static TokenizationValidationSnapshot CreateSnapshot(string text, EncodingResult encoding)
    {
        var textHash = ParityHashUtilities.HashString(text);
        var encodingSummary = ParityHashUtilities.CreateSummary(encoding);
        var encodedJson = JsonSerializer.Serialize(encodingSummary, SummarySerializerOptions);
        var encodedHash = ComputeSha256(encodedJson);
        return new TokenizationValidationSnapshot
        {
            TextHash = textHash,
            EncodingHash = encodedHash,
        };
    }

    private static string ComputeSha256(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        using var sha = SHA256.Create();
        var buffer = Utf8.GetBytes(payload);
        return Convert.ToHexString(sha.ComputeHash(buffer)).ToLowerInvariant();
    }

    private static IReadOnlyList<string> ResolveModelIdentifiers()
    {
        var filter = Environment.GetEnvironmentVariable(ModelFilterEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var selected = filter
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static name => !string.IsNullOrWhiteSpace(name))
                .Select(static name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (selected.Length == 0)
            {
                throw new InvalidOperationException($"Environment variable '{ModelFilterEnvironmentVariable}' does not contain any model identifiers.");
            }

            return selected;
        }

        var root = TestDataPath.GetBenchmarksDataRoot();
        if (!Directory.Exists(root))
        {
            throw new InvalidOperationException($"Hugging Face benchmark directory not found at '{root}'.");
        }

        return Directory.EnumerateDirectories(root)
            .Where(static directory => File.Exists(Path.Combine(directory, "tokenizer.json")))
            .Select(static directory => Path.GetFileName(directory) ?? string.Empty)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
