namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class SentencePiecePythonParityIntegrationTests
{
    private const string BenchmarkFileName = "python-sentencepiece-benchmark.json";
    private const string DotnetBenchmarkFileName = "dotnet-sentencepiece-benchmark.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public static IEnumerable<object[]> ModelIdentifiers()
    {
        var root = RepositoryTestData.GetRoot();
        if (!Directory.Exists(root))
        {
            yield break;
        }

        foreach (var modelId in Directory.EnumerateDirectories(root)
                     .Select(Path.GetFileName)
                     .Where(static value => !string.IsNullOrEmpty(value))
                     .OrderBy(static value => value, StringComparer.Ordinal))
        {
            var benchmarkPath = Path.Combine(root, modelId!, BenchmarkFileName);
            if (File.Exists(benchmarkPath))
            {
                yield return new object[] { modelId! };
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
        var benchmarkPath = Path.Combine(modelRoot, BenchmarkFileName);
        Assert.True(File.Exists(benchmarkPath), $"Benchmark fixture missing for model '{modelId}'. Expected at {benchmarkPath}.");

        PythonBenchmarkModel benchmark;
        using (var stream = File.OpenRead(benchmarkPath))
        {
            benchmark = JsonSerializer.Deserialize<PythonBenchmarkModel>(stream, SerializerOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize Python benchmark for model '{modelId}'.");
        }

        var modelPath = ResolveModelAsset(benchmark.Metadata, modelRoot);
        var modelBytes = File.ReadAllBytes(modelPath);

        using var processor = SentencePieceModelFixture.CreateProcessor(modelBytes);
        var processorAssemblyVersion = processor.GetType().Assembly.GetName().Version?.ToString() ?? "unknown";
        var dotnetCases = new List<DotnetBenchmarkCaseSnapshot>(benchmark.Cases.Count);

        foreach (var testCase in benchmark.Cases)
        {
            dotnetCases.Add(ValidateCase(processor, testCase));
        }

        var dotnetBenchmarkPath = Path.Combine(modelRoot, DotnetBenchmarkFileName);
        SentencePieceDotnetBenchmarkWriter.Write(dotnetBenchmarkPath, benchmark, dotnetCases, processorAssemblyVersion);
    }

    private static DotnetBenchmarkCaseSnapshot ValidateCase(SentencePieceProcessor processor, PythonBenchmarkCase testCase)
    {
        var singleSnapshot = ValidateSingle(processor, testCase);
        var batchSnapshot = ValidateBatch(processor, testCase);

        return new DotnetBenchmarkCaseSnapshot
        {
            ContractId = testCase.ContractId,
            Length = testCase.Length,
            Description = testCase.Description,
            Options = testCase.Options ?? PythonBenchmarkCaseOptions.Default,
            Single = singleSnapshot,
            Batch = batchSnapshot
        };
    }

    private static DotnetBenchmarkSingleSnapshot ValidateSingle(SentencePieceProcessor processor, PythonBenchmarkCase testCase)
    {
        var single = testCase.Single;
        Assert.Equal(single.TextHash, ParityHashUtilities.HashString(single.Text));
        Assert.True(string.IsNullOrEmpty(single.PairText), "SentencePiece fixtures should not include pair inputs.");
        Assert.True(string.IsNullOrEmpty(single.PairTextHash), "SentencePiece fixtures should not include pair hashes.");

        var ids = processor.EncodeIds(single.Text);
        var pieces = processor.EncodePieces(single.Text);

        var encodingSnapshot = CreateEncodingSnapshot(ids, pieces);
        var encodingSummary = CreateEncodingSummary(encodingSnapshot);
        AssertEncodingSummary(single.Encoding, encodingSummary);

        var decodedFromIds = processor.DecodeIds(ids);
        Assert.Equal(single.DecodedHash, ParityHashUtilities.HashString(decodedFromIds));

        var decodedFromPieces = processor.DecodePieces(pieces);
        Assert.Equal(single.TextHash, ParityHashUtilities.HashString(decodedFromPieces));

        return new DotnetBenchmarkSingleSnapshot
        {
            Text = single.Text,
            PairText = null,
            Encoding = encodingSnapshot,
            Decoded = decodedFromIds
        };
    }

    private static DotnetBenchmarkBatchSnapshot ValidateBatch(SentencePieceProcessor processor, PythonBenchmarkCase testCase)
    {
        var batch = testCase.Batch;
        var batchTexts = BuildBatchInputs(testCase);
        Assert.Equal(batch.Count, batchTexts.Count);
        Assert.Equal(batch.TextsHash, ParityHashUtilities.HashStringSequence(batchTexts));
        Assert.True(batch.PairTexts is null || batch.PairTexts.Count == 0, "SentencePiece fixtures should not include pair batches.");
        Assert.True(string.IsNullOrEmpty(batch.PairTextsHash), "SentencePiece fixtures should not include pair batch hashes.");

        var idsBatch = processor.EncodeIds(batchTexts).ToList();
        var piecesBatch = processor.EncodePieces(batchTexts).ToList();
        Assert.Equal(batch.Encodings.Count, idsBatch.Count);
        Assert.Equal(idsBatch.Count, piecesBatch.Count);

        var encodingSnapshots = new DotnetEncodingSnapshot[idsBatch.Count];
        for (var index = 0; index < idsBatch.Count; index++)
        {
            var snapshot = CreateEncodingSnapshot(idsBatch[index], piecesBatch[index]);
            var summary = CreateEncodingSummary(snapshot);
            AssertEncodingSummary(batch.Encodings[index], summary);
            encodingSnapshots[index] = snapshot;
        }

        var decodedFromIds = processor.DecodeIds(idsBatch).ToList();
        Assert.Equal(batch.DecodedHash, ParityHashUtilities.HashStringSequence(decodedFromIds));

        var decodedFromPieces = processor.DecodePieces(piecesBatch).ToList();
        Assert.Equal(batch.TextsHash, ParityHashUtilities.HashStringSequence(decodedFromPieces));

        return new DotnetBenchmarkBatchSnapshot
        {
            Texts = batchTexts.ToArray(),
            PairTexts = null,
            Encodings = encodingSnapshots,
            Decoded = decodedFromIds
        };
    }

    private static DotnetEncodingSnapshot CreateEncodingSnapshot(IReadOnlyList<int> ids, IReadOnlyList<string> pieces)
    {
        var length = ids.Count;
        return new DotnetEncodingSnapshot
        {
            Length = length,
            Ids = ids.ToArray(),
            Tokens = pieces.ToArray(),
            TypeIds = CreateFilledArray(length, 0u),
            AttentionMask = CreateFilledArray(length, 1u),
            SpecialTokensMask = new uint[length],
            Offsets = CreateOffsets(length)
                .Select(static offset => new DotnetEncodingOffset { Start = offset.Start, End = offset.End })
                .ToArray(),
            WordIds = new int?[length],
            SequenceIds = CreateSequenceIds(length),
            Overflowing = Array.Empty<DotnetEncodingSnapshot>()
        };
    }

    private static EncodingSummary CreateEncodingSummary(DotnetEncodingSnapshot snapshot)
    {
        var offsets = snapshot.Offsets.Select(static offset => (offset.Start, offset.End)).ToArray();

        return new EncodingSummary
        {
            Length = snapshot.Length,
            IdsHash = ParityHashUtilities.HashInt32Sequence(snapshot.Ids),
            TokensHash = ParityHashUtilities.HashStringSequence(snapshot.Tokens),
            TypeIdsHash = ParityHashUtilities.HashUInt32Sequence(snapshot.TypeIds),
            AttentionMaskHash = ParityHashUtilities.HashUInt32Sequence(snapshot.AttentionMask),
            SpecialTokensMaskHash = ParityHashUtilities.HashUInt32Sequence(snapshot.SpecialTokensMask),
            OffsetsHash = ParityHashUtilities.HashOffsets(offsets),
            WordIdsHash = ParityHashUtilities.HashOptionalInt32Sequence(snapshot.WordIds),
            SequenceIdsHash = ParityHashUtilities.HashOptionalInt32Sequence(snapshot.SequenceIds),
            Overflowing = Array.Empty<EncodingSummary>()
        };
    }

    private static IReadOnlyList<string> BuildBatchInputs(PythonBenchmarkCase testCase)
    {
        if (testCase.Batch.Texts.Count > 0)
        {
            return testCase.Batch.Texts;
        }

        var inputs = new string[testCase.Batch.Count];
        for (var index = 0; index < inputs.Length; index++)
        {
            inputs[index] = $"{testCase.Single.Text} [sample:{index:D2}]";
        }

        return inputs;
    }

    private static uint[] CreateFilledArray(int length, uint value)
    {
        var buffer = new uint[length];
        if (length == 0)
        {
            return buffer;
        }

        Array.Fill(buffer, value);
        return buffer;
    }

    private static (int Start, int End)[] CreateOffsets(int length)
    {
        var offsets = new (int Start, int End)[length];
        for (var index = 0; index < offsets.Length; index++)
        {
            offsets[index] = (0, 0);
        }

        return offsets;
    }

    private static int?[] CreateSequenceIds(int length)
    {
        var sequenceIds = new int?[length];
        for (var index = 0; index < sequenceIds.Length; index++)
        {
            sequenceIds[index] = 0;
        }

        return sequenceIds;
    }

    private static string ResolveModelAsset(PythonBenchmarkMetadata metadata, string modelRoot)
    {
        if (!metadata.Assets.TryGetValue("model", out var relativePath))
        {
            throw new InvalidOperationException("Python benchmark metadata is missing the 'model' asset entry.");
        }

        var candidate = Path.GetFullPath(Path.Combine(modelRoot, relativePath));
        if (!File.Exists(candidate))
        {
            throw new FileNotFoundException($"Model asset referenced by benchmark is missing: {candidate}");
        }

        return candidate;
    }

    private static void AssertEncodingSummary(EncodingSummary expected, EncodingSummary actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        Assert.Equal(expected.IdsHash, actual.IdsHash);
        Assert.Equal(expected.TokensHash, actual.TokensHash);
        Assert.Equal(expected.TypeIdsHash, actual.TypeIdsHash);
        Assert.Equal(expected.AttentionMaskHash, actual.AttentionMaskHash);
        Assert.Equal(expected.SpecialTokensMaskHash, actual.SpecialTokensMaskHash);
        Assert.Equal(expected.OffsetsHash, actual.OffsetsHash);
        Assert.Equal(expected.WordIdsHash, actual.WordIdsHash);
        Assert.Equal(expected.SequenceIdsHash, actual.SequenceIdsHash);
        Assert.Equal(expected.Overflowing.Count, actual.Overflowing.Count);
    }
}
