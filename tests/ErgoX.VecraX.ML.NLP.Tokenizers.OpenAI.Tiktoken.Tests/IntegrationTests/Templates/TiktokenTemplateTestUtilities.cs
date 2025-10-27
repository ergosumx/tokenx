namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.IntegrationTests.Tiktoken.Templates;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;

internal static class TiktokenTemplateTestUtilities
{
    private const string TiktokenTarget = "tiktoken";

    private static readonly JsonSerializerOptions TemplateSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Lazy<IReadOnlyList<string>> TemplateFileNames = new(LoadTemplateFileNames);

    private static readonly ConcurrentDictionary<string, ValidationManifest> ManifestCache = new(StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<object[]> GetTemplateFileNames()
    {
        foreach (var fileName in TemplateFileNames.Value)
        {
            yield return new object[] { fileName };
        }
    }

    public static void AssertTemplateCase(string encodingFolder, string templateFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodingFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateFileName);

        var template = LoadTemplate(templateFileName);
        var manifest = GetManifest(encodingFolder);
        if (!manifest.Baselines.TryGetValue(template.Id, out var baseline))
        {
            throw new InvalidOperationException($"Validation manifest for '{encodingFolder}' does not define case '{template.Id}'.");
        }

        using var encoding = CreateEncoding(encodingFolder, manifest);

        var tokens = encoding.EncodeOrdinary(template.Text);

        var actualTextHash = ParityHashUtilities.HashString(template.Text);
        Assert.Equal(baseline.TextHash, actualTextHash);

        var actualEncodingHash = ParityHashUtilities.HashUInt32Sequence(tokens);
        Assert.Equal(baseline.EncodingHash, actualEncodingHash);
    }

    private static IReadOnlyList<string> LoadTemplateFileNames()
    {
        var templatesRoot = TestDataPath.GetTokenizationTemplatesRoot();
        if (!Directory.Exists(templatesRoot))
        {
            throw new DirectoryNotFoundException($"Template directory '{templatesRoot}' was not found.");
        }

        var results = new List<string>();
        foreach (var path in Directory.EnumerateFiles(templatesRoot, "tokenization-*.json", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            var skeleton = JsonSerializer.Deserialize<TemplateSkeleton>(File.ReadAllText(path), TemplateSerializerOptions);
            if (skeleton is null)
            {
                throw new InvalidOperationException($"Template '{fileName}' could not be deserialized.");
            }

            if (!SupportsTiktoken(skeleton.Targets))
            {
                continue;
            }

            results.Add(fileName);
        }

        results.Sort(StringComparer.Ordinal);
        return results;
    }

    private static TemplateDefinition LoadTemplate(string templateFileName)
    {
        var templatePath = Path.Combine(TestDataPath.GetTokenizationTemplatesRoot(), templateFileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file '{templatePath}' was not found.", templatePath);
        }

        var payload = JsonSerializer.Deserialize<TemplatePayload>(File.ReadAllText(templatePath), TemplateSerializerOptions);
        if (payload is null)
        {
            throw new InvalidOperationException($"Template '{templateFileName}' could not be deserialized.");
        }

        if (string.IsNullOrWhiteSpace(payload.Id))
        {
            throw new InvalidOperationException($"Template '{templateFileName}' does not define an 'id'.");
        }

        if (!SupportsTiktoken(payload.Targets))
        {
            throw new InvalidOperationException($"Template '{templateFileName}' is not flagged for TikToken coverage.");
        }

        var single = payload.Single ?? throw new InvalidOperationException($"Template '{templateFileName}' is missing a 'single' definition.");
        if (string.IsNullOrWhiteSpace(single.Text))
        {
            throw new InvalidOperationException($"Template '{templateFileName}' single.text value is empty.");
        }

        return new TemplateDefinition(payload.Id, single.Text);
    }

    private static ValidationManifest GetManifest(string encodingFolder)
    {
        return ManifestCache.GetOrAdd(encodingFolder, static folder => LoadManifest(folder));
    }

    private static ValidationManifest LoadManifest(string encodingFolder)
    {
        var manifestPath = TestDataPath.GetValidationManifestPath(encodingFolder);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Validation manifest '{manifestPath}' was not found.", manifestPath);
        }

        var payload = JsonSerializer.Deserialize<ValidationManifestPayload>(File.ReadAllText(manifestPath), TemplateSerializerOptions);
        if (payload is null)
        {
            throw new InvalidOperationException($"Validation manifest '{manifestPath}' could not be deserialized.");
        }

        if (string.IsNullOrWhiteSpace(payload.Pattern))
        {
            throw new InvalidOperationException($"Validation manifest '{manifestPath}' does not specify a tokenization pattern.");
        }

        var specialTokens = payload.SpecialTokens is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : new Dictionary<string, int>(payload.SpecialTokens, StringComparer.Ordinal);
        var baselines = new Dictionary<string, TemplateBaseline>(StringComparer.Ordinal);
        if (payload.Cases is null || payload.Cases.Count == 0)
        {
            throw new InvalidOperationException($"Validation manifest '{manifestPath}' does not contain any cases.");
        }

        foreach (var entry in payload.Cases)
        {
            var caseId = entry.Key;
            if (string.IsNullOrWhiteSpace(caseId))
            {
                continue;
            }

            var snapshot = entry.Value?.Python;
            if (snapshot is null)
            {
                throw new InvalidOperationException($"Validation manifest '{manifestPath}' case '{caseId}' does not define a python baseline.");
            }

            if (string.IsNullOrWhiteSpace(snapshot.TextHash) || string.IsNullOrWhiteSpace(snapshot.EncodingHash))
            {
                throw new InvalidOperationException($"Validation manifest '{manifestPath}' case '{caseId}' contains empty hash values.");
            }

            baselines[caseId] = new TemplateBaseline(snapshot.TextHash, snapshot.EncodingHash);
        }

        return new ValidationManifest(
            payload.Encoding ?? encodingFolder,
            payload.Pattern,
            payload.ExplicitVocabularySize,
            specialTokens,
            baselines);
    }

    private static TiktokenEncoding CreateEncoding(string encodingFolder, ValidationManifest manifest)
    {
        var mergeableRanksPath = TestDataPath.GetMergeableRanksPath(encodingFolder);
        if (!File.Exists(mergeableRanksPath))
        {
            throw new FileNotFoundException($"Mergeable ranks file '{mergeableRanksPath}' was not found.", mergeableRanksPath);
        }

        return TiktokenEncodingFactory.FromTiktokenFile(
            manifest.Encoding,
            manifest.Pattern,
            mergeableRanksPath,
            manifest.SpecialTokens,
            explicitVocabularySize: null);
    }

    private static bool SupportsTiktoken(IReadOnlyList<string>? targets)
    {
        if (targets is null || targets.Count == 0)
        {
            return true;
        }

        foreach (var target in targets)
        {
            if (string.Equals(target, TiktokenTarget, StringComparison.OrdinalIgnoreCase) || string.Equals(target, "*", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private sealed record TemplateDefinition(string Id, string Text);

    private sealed record TemplateSkeleton
    {
        [JsonPropertyName("targets")]
        public IReadOnlyList<string>? Targets { get; init; }
    }

    private sealed record TemplatePayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("targets")]
        public IReadOnlyList<string>? Targets { get; init; }

        [JsonPropertyName("single")]
        public TemplateSinglePayload? Single { get; init; }
    }

    private sealed record TemplateSinglePayload
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private sealed record ValidationManifest(
        string Encoding,
        string Pattern,
        uint? ExplicitVocabularySize,
        IReadOnlyDictionary<string, int> SpecialTokens,
        IReadOnlyDictionary<string, TemplateBaseline> Baselines);

    private sealed record ValidationManifestPayload
    {
        [JsonPropertyName("encoding")]
        public string? Encoding { get; init; }

        [JsonPropertyName("pattern")]
        public string? Pattern { get; init; }

        [JsonPropertyName("explicitVocabularySize")]
        public uint? ExplicitVocabularySize { get; init; }

        [JsonPropertyName("specialTokens")]
        public Dictionary<string, int>? SpecialTokens { get; init; }

        [JsonPropertyName("cases")]
        public Dictionary<string, ValidationCasePayload?>? Cases { get; init; }
    }

    private sealed record ValidationCasePayload
    {
        [JsonPropertyName("py")]
        public ValidationSnapshotPayload? Python { get; init; }
    }

    private sealed record ValidationSnapshotPayload
    {
        [JsonPropertyName("text-hash")]
        public string? TextHash { get; init; }

        [JsonPropertyName("encoding-hash")]
        public string? EncodingHash { get; init; }
    }

    private readonly record struct TemplateBaseline(string TextHash, string EncodingHash);
}
