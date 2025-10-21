namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class EncodingPythonParityIntegrationTests : HuggingFaceTestBase
{
    private const string SolutionFileName = "TokenX.HF.sln";

    public static IEnumerable<object[]> ModelIdentifiers()
    {
        var root = GetBenchmarksDataRoot();
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            var modelId = Path.GetFileName(directory);
            var pythonFixture = Path.Combine(directory, "python-benchmark.json");
            if (File.Exists(pythonFixture))
            {
                yield return [modelId];
            }
        }
    }

    [Theory]
    [MemberData(nameof(ModelIdentifiers))]
    public void Tokenizer_matches_python_reference(string modelFolder)
    {
        var benchmark = LoadBenchmark(modelFolder);
        var modelRoot = Path.Combine(GetBenchmarksDataRoot(), modelFolder);
        var tokenizerPath = ResolveAssetPath(modelRoot, benchmark.Metadata.Assets, "tokenizer.json");
        Assert.True(File.Exists(tokenizerPath), $"Tokenizer asset missing: {tokenizerPath}");

        using var tokenizer = Tokenizer.FromFile(tokenizerPath);
        var dotnetCases = new List<DotnetBenchmarkCaseSnapshot>();

        foreach (var testCase in benchmark.Cases)
        {
            Assert.Equal(ParityHashUtilities.HashString(testCase.Single.Text), testCase.Single.TextHash);
            if (testCase.Single.PairText is not null)
            {
                Assert.Equal(testCase.Single.PairTextHash, ParityHashUtilities.HashString(testCase.Single.PairText));
            }

            using var configurationScope = new TokenizerConfigurationScope(tokenizer, testCase.Options);

            var addSpecialTokens = testCase.Options.AddSpecialTokens;
            var skipSpecialTokens = testCase.Options.DecodeSkipSpecialTokens;

            var singleEncoding = testCase.Single.PairText is not null
                ? tokenizer.Encode(testCase.Single.Text, testCase.Single.PairText, addSpecialTokens)
                : tokenizer.Encode(testCase.Single.Text, addSpecialTokens);
            AssertEncoding(testCase.Single.Encoding, singleEncoding);

            var decoded = tokenizer.Decode(singleEncoding.Ids, skipSpecialTokens);
            var decodedHash = ParityHashUtilities.HashString(decoded);
            Assert.Equal(testCase.Single.DecodedHash, decodedHash);

            var batchTexts = BuildBatchInputs(testCase);
            Assert.Equal(testCase.Batch.Count, batchTexts.Count);
            Assert.Equal(testCase.Batch.TextsHash, ParityHashUtilities.HashStringSequence(batchTexts));

            IReadOnlyList<string>? batchPairTexts = null;
            if (testCase.Batch.PairTexts is { Count: > 0 } pairs)
            {
                Assert.Equal(testCase.Batch.Count, pairs.Count);
                Assert.Equal(testCase.Batch.PairTextsHash, ParityHashUtilities.HashStringSequence(pairs));
                batchPairTexts = pairs;
            }

            IReadOnlyList<EncodingResult> batchEncodings;
            if (batchPairTexts is not null)
            {
                var sequencePairs = batchTexts
                    .Zip(batchPairTexts, static (first, second) => (first, (string?)second))
                    .ToArray();
                batchEncodings = tokenizer.EncodeBatch(sequencePairs, addSpecialTokens);
            }
            else
            {
                batchEncodings = tokenizer.EncodeBatch(batchTexts, addSpecialTokens);
            }

            Assert.Equal(testCase.Batch.Count, batchEncodings.Count);

            for (var i = 0; i < batchEncodings.Count; i++)
            {
                AssertEncoding(testCase.Batch.Encodings[i], batchEncodings[i]);
            }

            var decodedBatch = tokenizer.DecodeBatch(
                batchEncodings.Select(static e => e.Ids).ToArray(),
                skipSpecialTokens);
            var decodedBatchList = decodedBatch.ToList();
            Assert.Equal(testCase.Batch.DecodedHash, ParityHashUtilities.HashStringSequence(decodedBatchList));

            dotnetCases.Add(
                DotnetBenchmarkWriter.CreateCaseSnapshot(
                    testCase,
                    singleEncoding,
                    decoded,
                    batchTexts,
                    batchPairTexts,
                    batchEncodings,
                    decodedBatchList));
        }

        var dotnetBenchmarkPath = Path.Combine(modelRoot, "dotnet-benchmark.json");
        DotnetBenchmarkWriter.Write(dotnetBenchmarkPath, benchmark, dotnetCases);
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

    private static void AssertEncoding(EncodingSummary expected, EncodingResult actual)
    {
        var actualSummary = ParityHashUtilities.CreateSummary(actual);
        AssertEncodingSummary(expected, actualSummary);
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

        for (var i = 0; i < expected.Overflowing.Count; i++)
        {
            AssertEncodingSummary(expected.Overflowing[i], actual.Overflowing[i]);
        }
    }

    private static PythonBenchmarkModel LoadBenchmark(string modelFolder)
    {
        var path = Path.Combine(GetBenchmarksDataRoot(), modelFolder, "python-benchmark.json");
        Assert.True(File.Exists(path), $"Benchmark fixture missing. Run generate_benchmarks.py (expected at {path}).");

        using var stream = File.OpenRead(path);
        var model = JsonSerializer.Deserialize<PythonBenchmarkModel>(stream);
        return model ?? throw new InvalidOperationException($"Failed to deserialize benchmark at {path}.");
    }

    private static string ResolveAssetPath(string modelRoot, IReadOnlyDictionary<string, string> assets, string key)
    {
        if (!assets.TryGetValue(key, out var relativePath))
        {
            throw new InvalidOperationException($"Benchmark metadata is missing asset '{key}'.");
        }

        return Path.GetFullPath(Path.Combine(modelRoot, relativePath));
    }

    private static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_TestData");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }

    private sealed class TokenizerConfigurationScope : IDisposable
    {
        private readonly Tokenizer tokenizer;
        private readonly TruncationOptions? originalTruncation;
        private readonly PaddingOptions? originalPadding;

        public TokenizerConfigurationScope(Tokenizer tokenizer, PythonBenchmarkCaseOptions options)
        {
            this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            options ??= PythonBenchmarkCaseOptions.Default;

            originalPadding = this.tokenizer.GetPadding();
            originalTruncation = tokenizer.GetTruncation();

            // Align with Python fixture generation which calls backend.no_padding().
            this.tokenizer.DisablePadding();

            if (options.Truncation is not null)
            {
                var active = new TruncationOptions(options.Truncation.MaxLength, options.Truncation.Stride);
                this.tokenizer.EnableTruncation(active);
            }
            else
            {
                this.tokenizer.DisableTruncation();
            }
        }

        public void Dispose()
        {
            if (originalTruncation is not null)
            {
                var restore = new TruncationOptions(
                    originalTruncation.MaxLength,
                    originalTruncation.Stride,
                    originalTruncation.Strategy,
                    originalTruncation.Direction);
                tokenizer.EnableTruncation(restore);
            }
            else
            {
                tokenizer.DisableTruncation();
            }

            if (originalPadding is not null)
            {
                tokenizer.EnablePadding(originalPadding);
            }
            else
            {
                tokenizer.DisablePadding();
            }
        }
    }
}
