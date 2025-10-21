namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class SentencePiecePythonParityIntegrationTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public static IEnumerable<object[]> ModelIdentifiers()
    {
        var root = RepositoryTestData.GetRoot();
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            var modelId = Path.GetFileName(directory);
            if (string.IsNullOrEmpty(modelId))
            {
                continue;
            }

            var benchmarkPath = Path.Combine(directory, "python-sentencepiece-benchmark.json");
            if (File.Exists(benchmarkPath))
            {
                yield return [modelId];
            }
        }
    }

    [Theory]
    [MemberData(nameof(ModelIdentifiers))]
    public void Processor_matches_python_reference(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("Model identifier must be provided.", nameof(modelId));
        }

        var dataRoot = RepositoryTestData.GetRoot();
        var modelRoot = Path.Combine(dataRoot, modelId);
        var benchmarkPath = Path.Combine(modelRoot, "python-sentencepiece-benchmark.json");
        Assert.True(File.Exists(benchmarkPath), $"Benchmark fixture missing for model '{modelId}'. Expected at {benchmarkPath}.");

        SentencePiecePythonBenchmark benchmark;
        using (var stream = File.OpenRead(benchmarkPath))
        {
            benchmark = JsonSerializer.Deserialize<SentencePiecePythonBenchmark>(stream, SerializerOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize Python benchmark for model '{modelId}'.");
        }

        if (!benchmark.Metadata.Assets.TryGetValue("model", out var modelFile))
        {
            throw new InvalidOperationException($"Python benchmark metadata for '{modelId}' is missing the 'model' asset entry.");
        }

        var modelPath = Path.Combine(modelRoot, modelFile);
        Assert.True(File.Exists(modelPath), $"Model asset referenced by benchmark is missing: {modelPath}");

        var modelBytes = File.ReadAllBytes(modelPath);
        using var processor = SentencePieceModelFixture.CreateProcessor(modelBytes);

        foreach (var testCase in benchmark.Cases)
        {
            VerifySingle(processor, testCase);
            VerifyBatch(processor, testCase);
        }
    }

    private static void VerifySingle(SentencePieceProcessor processor, SentencePiecePythonBenchmarkCase testCase)
    {
        var ids = processor.EncodeIds(testCase.Text);
        Assert.Equal(testCase.Ids, ids);

        var pieces = processor.EncodePieces(testCase.Text);
        Assert.Equal(testCase.Pieces, pieces);

        var decodedFromIds = processor.DecodeIds(ids);
        Assert.Equal(testCase.Decoded, decodedFromIds);

        var decodedFromPieces = processor.DecodePieces(pieces);
        Assert.Equal(testCase.Decoded, decodedFromPieces);
    }

    private static void VerifyBatch(SentencePieceProcessor processor, SentencePiecePythonBenchmarkCase testCase)
    {
        if (testCase.BatchTexts.Count == 0)
        {
            return;
        }

        var idsBatch = processor.EncodeIds(testCase.BatchTexts);
        Assert.Equal(testCase.BatchIds.Count, idsBatch.Count);
        for (var index = 0; index < idsBatch.Count; index++)
        {
            Assert.Equal(testCase.BatchIds[index], idsBatch[index]);
        }

        var piecesBatch = processor.EncodePieces(testCase.BatchTexts);
        Assert.Equal(testCase.BatchPieces.Count, piecesBatch.Count);
        for (var index = 0; index < piecesBatch.Count; index++)
        {
            Assert.Equal(testCase.BatchPieces[index], piecesBatch[index]);
        }

        var decodedFromIds = processor.DecodeIds(idsBatch);
        Assert.Equal(testCase.BatchDecoded, decodedFromIds);

        var decodedFromPieces = processor.DecodePieces(piecesBatch);
        Assert.Equal(testCase.BatchDecoded, decodedFromPieces);
    }

    private sealed record SentencePiecePythonBenchmark
    {
        [JsonPropertyName("metadata")]
        public SentencePiecePythonBenchmarkMetadata Metadata { get; init; } = default!;

        [JsonPropertyName("cases")]
        public IReadOnlyList<SentencePiecePythonBenchmarkCase> Cases { get; init; } = Array.Empty<SentencePiecePythonBenchmarkCase>();
    }

    private sealed record SentencePiecePythonBenchmarkMetadata
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; init; } = string.Empty;

        [JsonPropertyName("repo_id")]
        public string? RepoId { get; init; }

        [JsonPropertyName("generated_at")]
        public string GeneratedAt { get; init; } = string.Empty;

        [JsonPropertyName("sentencepiece_version")]
        public string SentencePieceVersion { get; init; } = string.Empty;

        [JsonPropertyName("assets")]
        public IReadOnlyDictionary<string, string> Assets { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private sealed record SentencePiecePythonBenchmarkCase
    {
        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;

        [JsonPropertyName("ids")]
        public IReadOnlyList<int> Ids { get; init; } = Array.Empty<int>();

        [JsonPropertyName("pieces")]
        public IReadOnlyList<string> Pieces { get; init; } = Array.Empty<string>();

        [JsonPropertyName("decoded")]
        public string Decoded { get; init; } = string.Empty;

        [JsonPropertyName("batchTexts")]
        public IReadOnlyList<string> BatchTexts { get; init; } = Array.Empty<string>();

        [JsonPropertyName("batchIds")]
        public IReadOnlyList<IReadOnlyList<int>> BatchIds { get; init; } = Array.Empty<IReadOnlyList<int>>();

        [JsonPropertyName("batchPieces")]
        public IReadOnlyList<IReadOnlyList<string>> BatchPieces { get; init; } = Array.Empty<IReadOnlyList<string>>();

        [JsonPropertyName("batchDecoded")]
        public IReadOnlyList<string> BatchDecoded { get; init; } = Array.Empty<string>();
    }
}
