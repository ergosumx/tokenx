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
    private const string ContractTarget = "sentencepiece";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    private static readonly Lazy<IReadOnlyDictionary<string, TokenizationContractCase>> ContractCases =
        new(LoadContractCases);

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
            var contractCase = ResolveContractCase(testCase);
            AssertContractAlignment(contractCase, testCase);
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

    private static IReadOnlyDictionary<string, TokenizationContractCase> LoadContractCases()
    {
        var contractPath = Path.Combine(GetTemplatesRoot(), "tokenization-cases.json");
        if (!File.Exists(contractPath))
        {
            throw new InvalidOperationException($"Tokenization contract not found at '{contractPath}'.");
        }

        using var stream = File.OpenRead(contractPath);
        var contract = JsonSerializer.Deserialize<TokenizationContract>(stream)
            ?? throw new InvalidOperationException($"Failed to deserialize tokenization contract at '{contractPath}'.");

        return contract.Cases.ToDictionary(static entry => entry.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetTemplatesRoot()
    {
        var testDataRoot = RepositoryTestData.GetRoot();
        var testsRoot = Directory.GetParent(testDataRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate tests directory from repository layout.");

        return Path.Combine(testsRoot, "_templates");
    }

    private static TokenizationContractCase ResolveContractCase(PythonBenchmarkCase testCase)
    {
        if (testCase is null)
        {
            throw new ArgumentNullException(nameof(testCase));
        }

        var cases = ContractCases.Value;
        if (!string.IsNullOrWhiteSpace(testCase.ContractId))
        {
            if (cases.TryGetValue(testCase.ContractId, out var explicitMatch))
            {
                return explicitMatch;
            }

            throw new InvalidOperationException(
                $"Tokenization contract is missing case '{testCase.ContractId}'. Regenerate Python fixtures.");
        }

        var fallback = cases.Values
            .Where(static entry => entry.AppliesToTarget(ContractTarget))
            .Where(entry => string.Equals(entry.Length, testCase.Length, StringComparison.Ordinal))
            .Where(entry => string.Equals(entry.Single.TextHash, testCase.Single.TextHash, StringComparison.Ordinal))
            .Where(entry => string.Equals(entry.Batch.TextsHash, testCase.Batch.TextsHash, StringComparison.Ordinal))
            .Take(2)
            .ToList();

        if (fallback.Count == 1)
        {
            return fallback[0];
        }

        throw new InvalidOperationException(
            "Unable to resolve tokenization contract case. Regenerate fixtures to embed contract identifiers.");
    }

    private static void AssertContractAlignment(TokenizationContractCase contractCase, PythonBenchmarkCase testCase)
    {
        if (!contractCase.AppliesToTarget(ContractTarget))
        {
            throw new InvalidOperationException(
                $"Contract case '{contractCase.Id}' is not registered for target '{ContractTarget}'.");
        }

        Assert.Equal(contractCase.Length, testCase.Length);
        Assert.Equal(contractCase.Description, testCase.Description);

        var options = testCase.Options ?? PythonBenchmarkCaseOptions.Default;
        Assert.Equal(contractCase.Options.AddSpecialTokens, options.AddSpecialTokens);
        Assert.Equal(contractCase.Options.DecodeSkipSpecialTokens, options.DecodeSkipSpecialTokens);
        Assert.Null(contractCase.Options.Truncation);
        Assert.Null(options.Truncation);

        Assert.Equal(contractCase.Single.Text, testCase.Single.Text);
        Assert.Equal(contractCase.Single.TextHash, testCase.Single.TextHash);
        Assert.Equal(contractCase.Single.TextHash, ParityHashUtilities.HashString(contractCase.Single.Text));
        Assert.Null(contractCase.Single.PairText);
        Assert.Null(contractCase.Single.PairTextHash);
        Assert.True(string.IsNullOrEmpty(testCase.Single.PairText));
        Assert.True(string.IsNullOrEmpty(testCase.Single.PairTextHash));

        Assert.Equal(contractCase.Batch.Count, testCase.Batch.Count);
        Assert.Equal(contractCase.Batch.TextsHash, testCase.Batch.TextsHash);
        Assert.Equal(contractCase.Batch.Texts, testCase.Batch.Texts);
        Assert.Equal(
            contractCase.Batch.TextsHash,
            ParityHashUtilities.HashStringSequence(contractCase.Batch.Texts));

        Assert.True(contractCase.Batch.PairTexts is null || contractCase.Batch.PairTexts.Count == 0);
        Assert.True(string.IsNullOrEmpty(contractCase.Batch.PairTextsHash));
        Assert.True(testCase.Batch.PairTexts is null || testCase.Batch.PairTexts.Count == 0);
        Assert.True(string.IsNullOrEmpty(testCase.Batch.PairTextsHash));
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
