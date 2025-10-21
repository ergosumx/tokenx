namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class SentencePieceProcessorIntegrationTests : IClassFixture<SentencePieceModelFixture>
{
    private readonly SentencePieceModelFixture fixture;

    public SentencePieceProcessorIntegrationTests(SentencePieceModelFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void EncodeIds_RoundTrips()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        const string input = "Hello SentencePiece!";

        var ids = processor.EncodeIds(input);
        Assert.NotEmpty(ids);

        var reconstructed = processor.DecodeIds(ids);
        Assert.False(string.IsNullOrWhiteSpace(reconstructed));

        var reEncoded = processor.EncodeIds(reconstructed);
        Assert.Equal(ids, reEncoded);
    }

    [Fact]
    public void EncodePieces_ProvidesSurfaceForms()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        const string input = "Testing multilingual tokenization.";

        var pieces = processor.EncodePieces(input);
        Assert.NotEmpty(pieces);
        Assert.Contains(pieces, piece => piece.Contains("▁", StringComparison.Ordinal));
    }

    [Fact]
    public void PieceCountAliases_AreConsistent()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        Assert.Equal(processor.PieceCount, processor.PieceSize);
        Assert.Equal(processor.PieceCount, processor.VocabSize);
    }

    [Fact]
    public void BatchAliases_MirrorSingleOperations()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var pieces = new List<string> { "▁Test", "ing" };

        var single = pieces.Select(processor.PieceToId).ToArray();
        var batch = processor.PieceToIds(pieces);

        Assert.Equal(single, batch);
        Assert.Equal(pieces, processor.IdToPieces(single));
    }

    [Fact]
    public void NBestEncode_ProducesMultipleCandidates()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        const string input = "I love working with SentencePiece.";
        const int nbest = 4;

        var results = processor.NBestEncodeIds(input, nbest);
        Assert.Equal(nbest, results.Count);
        Assert.All(results, candidate => Assert.NotEmpty(candidate));
        Assert.True(results.Skip(1).Any(candidate => !candidate.SequenceEqual(results[0])), "Expected multiple distinct candidates.");
    }

    [Fact]
    public void EntropyBatch_ComputesScores()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var inputs = new[] { "Short", "A slightly longer sentence." };

        var entropies = processor.CalculateEntropyBatch(inputs, alpha: 0.5f, numThreads: 2);
        Assert.Equal(inputs.Length, entropies.Count);
        Assert.All(entropies, value => Assert.True(value >= 0));
    }

    [Fact]
    public void CalculateEntropyConvenience_DelegatesToBatch()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var inputs = new[] { "delegation" };

        var viaBatch = processor.CalculateEntropyBatch(inputs, alpha: 0.25f, numThreads: 0);
        var viaConvenience = processor.CalculateEntropy(inputs, alpha: 0.25f);

        Assert.Equal(viaBatch, viaConvenience);
    }

    [Fact]
    public void SampleEncodeIds_VariesWithSeed()
    {
        const string input = "Sampling requires determinism.";

        SentencePieceEnvironment.SetRandomGeneratorSeed(123u);
        using var processorA = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var first = processorA.SampleEncodeIds(input, nbestSize: -1, alpha: 0.5f);

        SentencePieceEnvironment.SetRandomGeneratorSeed(321u);
        using var processorB = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var second = processorB.SampleEncodeIds(input, nbestSize: -1, alpha: 0.5f);

        Assert.NotEmpty(first);
        Assert.NotEmpty(second);
        Assert.False(first.SequenceEqual(second));
    }

    [Fact]
    public void SampleEncodePieces_ForBatchReturnsResults()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new[] { "batch sample A", "batch sample B" };

        var results = processor.SampleEncodePieces(inputs, nbestSize: -1, alpha: 0.5f);

        Assert.Equal(inputs.Length, results.Count);
        Assert.All(results, list => Assert.NotEmpty(list));
    }

    [Fact]
    public void TokenizeAndDetokenize_RoundTrip()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        const string input = "Tokenize this sentence.";

        var tokens = processor.Tokenize(input);
        Assert.NotEmpty(tokens);

        var reconstructed = processor.Detokenize(tokens);
        Assert.False(string.IsNullOrWhiteSpace(reconstructed));
    }

    [Fact]
    public void SentencePieceEnvironment_AcceptsDataDirectory()
    {
        var directory = AppContext.BaseDirectory;

        var exception = Record.Exception(() => SentencePieceEnvironment.SetDataDirectory(directory));

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_CanBeInvokedMultipleTimes()
    {
        var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        processor.Dispose();

        var secondDispose = Record.Exception(processor.Dispose);

        Assert.Null(secondDispose);
    }

    [Fact]
    public void AccessAfterDispose_ThrowsObjectDisposed()
    {
        var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        processor.Dispose();

        Assert.Throws<ObjectDisposedException>(() => processor.PieceCount);
    }

    [Fact]
    public void NBestEncodeBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "Batch candidate one.", "Alternate phrasing." };
        const int nbest = 3;

        var batched = processor.NBestEncodeIds(inputs, nbest);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.NBestEncodeIds(inputs[i], nbest);
            Assert.Equal(expected.Count, batched[i].Count);
            for (int j = 0; j < expected.Count; ++j)
            {
                Assert.Equal(expected[j], batched[i][j]);
            }
        }
    }

    [Fact]
    public void NBestEncodePiecesBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "Batch piece one", "Batch piece two" };
        const int nbest = 2;

        var batched = processor.NBestEncodePieces(inputs, nbest);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.NBestEncodePieces(inputs[i], nbest);
            Assert.Equal(expected.Count, batched[i].Count);
            for (int j = 0; j < expected.Count; ++j)
            {
                Assert.Equal(expected[j], batched[i][j]);
            }
        }
    }

    [Fact]
    public void NBestEncodeSerializedProtoBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "Proto batch one", "Proto batch two" };
        const int nbest = 2;

        var batched = processor.NBestEncodeSerializedProto(inputs, nbest);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.NBestEncodeSerializedProto(inputs[i], nbest);
            Assert.Equal(expected, batched[i]);
        }
    }

    [Fact]
    public void SampleEncodeAndScoreBatch_ReturnsPerInputResults()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "sample batch one", "sample batch two" };
        var options = new SampleEncodeAndScoreOptions
        {
            NumSamples = 3,
            Alpha = 0.7f,
            WithoutReplacement = true,
            IncludeBest = true,
        };

        var batched = processor.SampleEncodeAndScoreIds(inputs, options);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.SampleEncodeAndScoreIds(inputs[i], options);
            Assert.Equal(expected.Count, batched[i].Count);
            Assert.All(batched[i], result => Assert.NotEmpty(result.Ids));
        }
    }

    [Fact]
    public void SampleEncodeAndScorePiecesBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "pieces batch one", "pieces batch two" };
        var options = new SampleEncodeAndScoreOptions
        {
            NumSamples = 2,
            Alpha = 0.6f,
            WithoutReplacement = true,
            IncludeBest = true,
        };

        var batched = processor.SampleEncodeAndScorePieces(inputs, options);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.SampleEncodeAndScorePieces(inputs[i], options);
            Assert.Equal(expected.Count, batched[i].Count);
            Assert.All(batched[i], result => Assert.NotEmpty(result.Pieces));
        }
    }

    [Fact]
    public void SampleEncodeAndScoreSerializedProtoBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var inputs = new List<string> { "proto sample one", "proto sample two" };
        var options = new SampleEncodeAndScoreOptions
        {
            NumSamples = 2,
            Alpha = 0.6f,
            WithoutReplacement = false,
            IncludeBest = true,
        };

        var batched = processor.SampleEncodeAndScoreSerializedProto(inputs, options);

        Assert.Equal(inputs.Count, batched.Count);
        for (int i = 0; i < inputs.Count; ++i)
        {
            var expected = processor.SampleEncodeAndScoreSerializedProto(inputs[i], options);
            Assert.Equal(expected, batched[i]);
        }
    }

    [Fact]
    public void NormalizeBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var inputs = new[] { "SentencePiece", "Normalizer" };

        var batched = processor.Normalize(inputs);

        Assert.Equal(inputs.Length, batched.Count);
        for (int i = 0; i < inputs.Length; ++i)
        {
            var expected = processor.Normalize(inputs[i]);
            Assert.Equal(expected, batched[i]);
        }
    }

    [Fact]
    public void NormalizeWithOffsetsBatch_DelegatesToSingle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var inputs = new[] { "Offsets", "Check" };

        var batched = processor.NormalizeWithOffsets(inputs);

        Assert.Equal(inputs.Length, batched.Count);
        for (int i = 0; i < inputs.Length; ++i)
        {
            var expected = processor.NormalizeWithOffsets(inputs[i]);
            Assert.Equal(expected.Text, batched[i].Text);
            Assert.Equal(expected.Offsets, batched[i].Offsets);
        }
    }

    [Fact]
    public void ConvenienceBatches_ReturnEmptyForEmptyInputs()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);

        Assert.Empty(processor.PieceToIds(Array.Empty<string>()));
        Assert.Empty(processor.IdToPieces(Array.Empty<int>()));
        Assert.Empty(processor.EncodeIds(Array.Empty<string>()));
        Assert.Empty(processor.EncodePieces(Array.Empty<string>()));
        Assert.Empty(processor.EncodeSerializedProto(Array.Empty<string>()));

        var emptyIds = Array.Empty<int[]>();
        var emptyPieces = Array.Empty<string[]>();

        Assert.Empty(processor.DecodeIds(emptyIds));
        Assert.Empty(processor.DecodePieces(emptyPieces));
        Assert.Empty(processor.DecodeIdsAsBytes(emptyIds));
        Assert.Empty(processor.DecodeIdsAsSerializedProto(emptyIds));
        Assert.Empty(processor.DecodePiecesAsSerializedProto(emptyPieces));
    }

    [Fact]
    public void AdvancedConvenienceBatches_ReturnEmptyForEmptyInputs()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var emptyInputs = Array.Empty<string>();

        Assert.Empty(processor.NBestEncodeIds(emptyInputs, nbestSize: 2));
        Assert.Empty(processor.NBestEncodePieces(emptyInputs, nbestSize: 2));
        Assert.Empty(processor.NBestEncodeSerializedProto(emptyInputs, nbestSize: 2));

        Assert.Empty(processor.SampleEncodeAndScoreIds(emptyInputs));
        Assert.Empty(processor.SampleEncodeAndScorePieces(emptyInputs));
        Assert.Empty(processor.SampleEncodeAndScoreSerializedProto(emptyInputs));

        Assert.Empty(processor.Normalize(emptyInputs));
        Assert.Empty(processor.NormalizeWithOffsets(emptyInputs));
    }

    [Fact]
    public void EncodeOperations_AcceptCustomOptions()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var options = new EncodeOptions();

        var ids = processor.EncodeIds("options coverage", options);
        Assert.NotEmpty(ids);

        var pieces = processor.EncodePieces("options coverage", options);
        Assert.NotEmpty(pieces);

        var proto = processor.EncodeSerializedProto("options coverage", options);
        Assert.NotEmpty(proto);

        var batchInputs = new List<string> { "one", "two" };

        var idBatch = processor.EncodeIdsBatch(batchInputs, numThreads: 1, options);
        Assert.Equal(batchInputs.Count, idBatch.Count);

        var pieceBatch = processor.EncodePiecesBatch(batchInputs, numThreads: 1, options);
        Assert.Equal(batchInputs.Count, pieceBatch.Count);

        var protoBatch = processor.EncodeSerializedProtoBatch(batchInputs, numThreads: 1, options);
        Assert.Equal(batchInputs.Count, protoBatch.Count);
    }

    [Fact]
    public void EncodeAndDecodeBatches_HandleNullInputs()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var emptyStrings = Array.Empty<string>();
        var emptyIds = Array.Empty<int>();
        var emptyIdSequences = Array.Empty<IReadOnlyList<int>>();
        var emptyPieceSequences = Array.Empty<IReadOnlyList<string>>();

        Assert.Equal(processor.EncodeIdsBatch(emptyStrings), processor.EncodeIdsBatch(inputs: null!));
        Assert.Equal(processor.EncodePiecesBatch(emptyStrings), processor.EncodePiecesBatch(inputs: null!));
        Assert.Equal(processor.EncodeSerializedProtoBatch(emptyStrings), processor.EncodeSerializedProtoBatch(inputs: null!));

        Assert.Equal(processor.DecodeIds(emptyIds), processor.DecodeIds(ids: null!));
        Assert.Equal(processor.DecodePieces(emptyStrings), processor.DecodePieces(pieces: null!));

        Assert.Equal(processor.DecodeIdsAsBytes(emptyIds), processor.DecodeIdsAsBytes(ids: null!));
        Assert.Equal(processor.DecodeIdsAsSerializedProto(emptyIds), processor.DecodeIdsAsSerializedProto(ids: null!));
        Assert.Equal(processor.DecodePiecesAsSerializedProto(emptyStrings), processor.DecodePiecesAsSerializedProto(pieces: null!));

        Assert.Equal(processor.DecodeIdsBatch(emptyIdSequences), processor.DecodeIdsBatch(inputs: null!));
        Assert.Equal(processor.DecodeIdsAsBytesBatch(emptyIdSequences), processor.DecodeIdsAsBytesBatch(inputs: null!));
        Assert.Equal(processor.DecodeIdsAsSerializedProtoBatch(emptyIdSequences), processor.DecodeIdsAsSerializedProtoBatch(inputs: null!));
        Assert.Equal(processor.DecodePiecesBatch(emptyPieceSequences), processor.DecodePiecesBatch(inputs: null!));
        Assert.Equal(processor.DecodePiecesAsSerializedProtoBatch(emptyPieceSequences), processor.DecodePiecesAsSerializedProtoBatch(inputs: null!));
    }

    [Fact]
    public void NormalizationHelpers_HandleNullInputs()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);

        var baselineEntropy = processor.CalculateEntropyBatch(Array.Empty<string>());
        var fromNull = processor.CalculateEntropyBatch(inputs: null!);
        Assert.Equal(baselineEntropy, fromNull);

        var exception = Record.Exception(() => processor.OverrideNormalizerSpec(null!));
        Assert.Null(exception);
    }

    [Fact]
    public void Processor_DisposedInstanceThrows()
    {
        var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        processor.Dispose();
        processor.Dispose();

        Assert.Throws<ObjectDisposedException>(() => processor.EncodeIds("disposed"));
        Assert.Throws<ObjectDisposedException>(() => processor.SetVocabulary(Array.Empty<string>()));
    }

    [Fact]
    public void VocabularyOperations_HandleNullInputs()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);

        var exception = Record.Exception(() => processor.SetVocabulary(null!));
        Assert.Null(exception);

        var reset = Record.Exception(() => processor.ResetVocabulary());
        Assert.Null(reset);
    }

    [Fact]
    public void LoadVocabulary_ReadsTsvFile()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.Mt5SmallModel);
        var vocabPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tsv");

        try
        {
            File.WriteAllLines(vocabPath, new[] { "token\t100", "another\t50" });
            var exception = Record.Exception(() => processor.LoadVocabulary(vocabPath, threshold: 0));
            Assert.Null(exception);
        }
        finally
        {
            if (File.Exists(vocabPath))
            {
                File.Delete(vocabPath);
            }
        }
    }

    [Fact]
    public void MetadataAccessors_ReturnConsistentValues()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);
        var pieces = processor.EncodePieces("metadata coverage");
        Assert.NotEmpty(pieces);

        var firstPiece = pieces[0];
        var firstId = processor.PieceToId(firstPiece);
        Assert.Equal(firstPiece, processor.IdToPiece(firstId));

        _ = processor.GetScore(firstId);
        _ = processor.IsControl(firstId);
        _ = processor.IsUnused(firstId);
        _ = processor.IsByte(firstId);
        _ = processor.PieceCount;
        _ = processor.UnknownId;
        _ = processor.BosId;
        _ = processor.EosId;
        _ = processor.PadId;

        if (processor.UnknownId >= 0)
        {
            Assert.True(processor.IsUnknown(processor.UnknownId));
        }

        var serialized = processor.SerializeModel();
        Assert.NotEmpty(serialized);
    }

    [Fact]
    public void SetDecodeExtraOptions_AllowsToggle()
    {
        using var processor = SentencePieceModelFixture.CreateProcessor(fixture.T5SmallModel);

        var setException = Record.Exception(() => processor.SetDecodeExtraOptions("reverse"));
        Assert.Null(setException);

        var resetException = Record.Exception(() => processor.SetDecodeExtraOptions(string.Empty));
        Assert.Null(resetException);
    }

    [Fact]
    public void SamplingOptionHelpers_CloneAndApplyOverrides()
    {
        var method = typeof(SentencePieceProcessor).GetMethod("PrepareSamplingOptions", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var baseOptions = new EncodeOptions
        {
            AddBos = true,
            AddEos = true,
            Reverse = true,
            EmitUnknownPiece = true,
            EnableSampling = false,
            NBestSize = 8,
            Alpha = 0.9f,
        };

        var prepared = (EncodeOptions)method!.Invoke(null, new object?[] { baseOptions, 5, 0.7f })!;
        Assert.NotSame(baseOptions, prepared);
        Assert.True(prepared.EnableSampling);
        Assert.Equal(5, prepared.NBestSize);
        Assert.Equal(0.7f, prepared.Alpha);
        Assert.True(prepared.AddBos);
        Assert.True(prepared.AddEos);
        Assert.True(prepared.EmitUnknownPiece);

        var fromNull = (EncodeOptions)method.Invoke(null, new object?[] { null, 3, 0.2f })!;
        Assert.True(fromNull.EnableSampling);
        Assert.Equal(3, fromNull.NBestSize);
        Assert.Equal(0.2f, fromNull.Alpha);
    }
}
