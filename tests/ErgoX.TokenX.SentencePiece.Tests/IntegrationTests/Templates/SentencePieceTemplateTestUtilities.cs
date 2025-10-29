namespace ErgoX.TokenX.SentencePiece.Tests.IntegrationTests.Templates;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.TokenX.SentencePiece.Processing;
using ErgoX.TokenX.Parity;
using Xunit;

internal static class SentencePieceTemplateTestUtilities
{
    private const string SentencePieceTarget = "sentencepiece";

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

    public static void AssertTemplateCase(ReadOnlyMemory<byte> modelBytes, string modelId, string templateFileName)
    {
        if (modelBytes.IsEmpty)
        {
            throw new ArgumentException("Model content must not be empty.", nameof(modelBytes));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateFileName);

        var template = LoadTemplate(templateFileName);
        var manifest = GetManifest(modelId);
        if (!manifest.Cases.TryGetValue(template.Id, out var baseline))
        {
            throw new InvalidOperationException($"Validation manifest for '{modelId}' does not define case '{template.Id}'.");
        }

        using var processor = new SentencePieceProcessor();
        processor.Load(modelBytes.Span);

        var ids = processor.EncodeIds(template.Text);
        var pieces = processor.EncodePieces(template.Text);
        var decodedFromIds = processor.DecodeIds(ids);
        var decodedFromPieces = processor.DecodePieces(pieces);

        var actualTextHash = ParityHashUtilities.HashString(template.Text);
        AssertHashEquals(modelId, template.Id, "text", baseline.TextHash, actualTextHash);

        var actualIdsHash = ParityHashUtilities.HashInt32Sequence(ids);
        AssertHashEquals(modelId, template.Id, "ids", baseline.IdsHash, actualIdsHash);

        var actualPiecesHash = ParityHashUtilities.HashStringSequence(pieces);
        AssertHashEquals(modelId, template.Id, "pieces", baseline.PiecesHash, actualPiecesHash);

    var actualDecodedFromIdsHash = ParityHashUtilities.HashString(decodedFromIds);
    AssertHashEquals(modelId, template.Id, "decoded-ids", baseline.DecodedIdsHash, actualDecodedFromIdsHash);

    var actualDecodedFromPiecesHash = ParityHashUtilities.HashString(decodedFromPieces);
    AssertHashEquals(modelId, template.Id, "decoded-pieces", baseline.DecodedPiecesHash, actualDecodedFromPiecesHash);
    }

    private static IReadOnlyList<string> LoadTemplateFileNames()
    {
        var templatesRoot = SentencePieceTestDataPath.GetTokenizationTemplatesRoot();
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

            if (!SupportsSentencePiece(skeleton.Targets))
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
        var templatePath = Path.Combine(SentencePieceTestDataPath.GetTokenizationTemplatesRoot(), templateFileName);
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

        if (!SupportsSentencePiece(payload.Targets))
        {
            throw new InvalidOperationException($"Template '{templateFileName}' is not flagged for SentencePiece coverage.");
        }

        var single = payload.Single ?? throw new InvalidOperationException($"Template '{templateFileName}' is missing a 'single' definition.");
        if (string.IsNullOrWhiteSpace(single.Text))
        {
            throw new InvalidOperationException($"Template '{templateFileName}' single.text value is empty.");
        }

        return new TemplateDefinition(payload.Id, single.Text);
    }

    private static ValidationManifest GetManifest(string modelId)
    {
        return ManifestCache.GetOrAdd(modelId, static key => LoadManifest(key));
    }

    private static ValidationManifest LoadManifest(string modelId)
    {
        var manifestPath = SentencePieceTestDataPath.GetManifestPath(modelId);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Validation manifest '{manifestPath}' was not found.", manifestPath);
        }

        var payload = JsonSerializer.Deserialize<ValidationManifestPayload>(File.ReadAllText(manifestPath), TemplateSerializerOptions);
        if (payload is null)
        {
            throw new InvalidOperationException($"Validation manifest '{manifestPath}' could not be deserialized.");
        }

        if (payload.Cases is null || payload.Cases.Count == 0)
        {
            throw new InvalidOperationException($"Validation manifest '{manifestPath}' does not contain any cases.");
        }

        var cases = new Dictionary<string, TemplateBaseline>(StringComparer.Ordinal);
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

            if (string.IsNullOrWhiteSpace(snapshot.TextHash) ||
                string.IsNullOrWhiteSpace(snapshot.IdsHash) ||
                string.IsNullOrWhiteSpace(snapshot.PiecesHash))
            {
                throw new InvalidOperationException($"Validation manifest '{manifestPath}' case '{caseId}' contains empty hash values.");
            }

            var decodedIdsHash = snapshot.DecodedIdsHash;
            if (string.IsNullOrWhiteSpace(decodedIdsHash))
            {
                decodedIdsHash = snapshot.LegacyDecodedHash;
            }

            var decodedPiecesHash = snapshot.DecodedPiecesHash;
            if (string.IsNullOrWhiteSpace(decodedPiecesHash))
            {
                decodedPiecesHash = snapshot.LegacyDecodedHash ?? decodedIdsHash;
            }

            if (string.IsNullOrWhiteSpace(decodedIdsHash) || string.IsNullOrWhiteSpace(decodedPiecesHash))
            {
                throw new InvalidOperationException($"Validation manifest '{manifestPath}' case '{caseId}' is missing decoded hash values.");
            }

            cases[caseId] = new TemplateBaseline(snapshot.TextHash, snapshot.IdsHash, snapshot.PiecesHash, decodedIdsHash, decodedPiecesHash);
        }

        return new ValidationManifest(payload.ModelId ?? modelId, payload.ModelFile ?? string.Empty, cases);
    }

    private static bool SupportsSentencePiece(IReadOnlyList<string>? targets)
    {
        if (targets is null || targets.Count == 0)
        {
            return true;
        }

        foreach (var target in targets)
        {
            if (string.Equals(target, SentencePieceTarget, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(target, "*", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertHashEquals(string modelId, string caseId, string label, string expected, string actual)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            Console.WriteLine($"[{modelId}:{caseId}] {label} mismatch -> expected {expected}, actual {actual}");
        }

        Assert.Equal(expected, actual);
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

    private sealed record ValidationManifest(string ModelId, string ModelFile, IReadOnlyDictionary<string, TemplateBaseline> Cases);

    private sealed record ValidationManifestPayload
    {
        [JsonPropertyName("model")]
        public string? ModelId { get; init; }

        [JsonPropertyName("modelFile")]
        public string? ModelFile { get; init; }

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

        [JsonPropertyName("ids-hash")]
        public string? IdsHash { get; init; }

        [JsonPropertyName("pieces-hash")]
        public string? PiecesHash { get; init; }

        [JsonPropertyName("decoded-ids-hash")]
        public string? DecodedIdsHash { get; init; }

        [JsonPropertyName("decoded-pieces-hash")]
        public string? DecodedPiecesHash { get; init; }

        [JsonPropertyName("decoded-hash")]
        public string? LegacyDecodedHash { get; init; }
    }

    private readonly record struct TemplateBaseline(string TextHash, string IdsHash, string PiecesHash, string DecodedIdsHash, string DecodedPiecesHash);
}

