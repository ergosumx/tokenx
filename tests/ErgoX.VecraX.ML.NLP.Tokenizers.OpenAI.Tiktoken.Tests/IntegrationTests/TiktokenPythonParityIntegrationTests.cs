namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.IntegrationTests.Tiktoken;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;
using Xunit.Sdk;

public sealed class TiktokenPythonParityIntegrationTests : TiktokenTestBase
{
    private const string BenchmarkFileName = "python-benchmark.json";
    private const string MergeableRanksFileName = "mergeable_ranks.tiktoken";
    private const string SolutionFileName = "TokenX.HF.sln";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static IEnumerable<object[]> ModelDirectories
    {
        get
        {
            var root = GetBenchmarksRoot();
            if (!Directory.Exists(root))
            {
                yield break;
            }

            foreach (var directory in Directory.EnumerateDirectories(root))
            {
                yield return new object[] { directory };
            }
        }
    }

    [Theory]
    [MemberData(nameof(ModelDirectories))]
    public void Parity_with_python_reference(string modelDirectory)
    {
        var benchmark = LoadBenchmark(modelDirectory);
        var metadata = benchmark.Metadata;

        var mergeableRanksPath = ResolveMergeableRanksPath(modelDirectory, metadata);

        using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
            metadata.Encoding,
            metadata.Pattern,
            mergeableRanksPath,
            metadata.SpecialTokens,
            explicitVocabularySize: null);

        var dotnetCases = new List<TiktokenDotnetBenchmarkCaseSnapshot>(benchmark.Cases.Count);

        foreach (var contractCase in benchmark.Cases)
        {
            var caseSnapshot = CreateCaseSnapshot(encoding, contractCase);
            dotnetCases.Add(caseSnapshot);
        }

        var dotnetBenchmarkPath = Path.Combine(modelDirectory, "dotnet-benchmark.json");
        var metadataSnapshot = new TiktokenDotnetBenchmarkMetadata
        {
            Encoding = metadata.Encoding,
            DisplayName = metadata.DisplayName,
            Model = metadata.Model,
            GeneratedAt = metadata.GeneratedAt,
            TiktokenVersion = metadata.TiktokenVersion,
            Pattern = metadata.Pattern,
            MergeableRanksFile = metadata.MergeableRanksFile ?? MergeableRanksFileName,
            SpecialTokens = metadata.SpecialTokens,
            ExplicitVocabularySize = metadata.ExplicitVocabularySize,
            PythonVocabularySize = metadata.PythonVocabularySize
        };

    var assemblyVersion = typeof(TiktokenEncoding).Assembly.GetName().Version?.ToString() ?? "unknown";
    TiktokenDotnetBenchmarkWriter.Write(dotnetBenchmarkPath, metadataSnapshot, dotnetCases, assemblyVersion);
    }

    private static TiktokenDotnetBenchmarkCaseSnapshot CreateCaseSnapshot(TiktokenEncoding encoding, PythonParityCase contractCase)
    {
        var single = contractCase.Single;
        Assert.NotNull(single);

        var singleTokens = encoding.EncodeOrdinary(single.Text);
        var singleTokenArray = ToInt32Array(singleTokens);

        Assert.Equal(single.Length, singleTokenArray.Length);
        Assert.Equal(single.TokenIds, singleTokenArray);
        Assert.Equal(single.TextHash, ParityHashUtilities.HashString(single.Text));

        var singleIdsHash = ParityHashUtilities.HashInt32Sequence(singleTokenArray);
        Assert.Equal(single.IdsHash, singleIdsHash);

        var singleDecoded = encoding.Decode(ToUInt32Array(singleTokenArray));
        var singleDecodedHash = ParityHashUtilities.HashString(singleDecoded);
        Assert.Equal(single.DecodedHash, singleDecodedHash);

        var batch = contractCase.Batch;
        Assert.NotNull(batch);
        Assert.Equal(batch.Count, batch.Texts.Count);
        Assert.Equal(batch.TextsHash, ParityHashUtilities.HashStringSequence(batch.Texts));
        Assert.Equal(batch.Texts.Count, batch.Encodings.Count);

        var decodedTexts = new List<string>(batch.Texts.Count);
        var batchSnapshots = new List<TiktokenDotnetEncodingSnapshot>(batch.Texts.Count);
        for (var index = 0; index < batch.Texts.Count; index++)
        {
            var text = batch.Texts[index];
            var expected = batch.Encodings[index];

            var tokens = encoding.EncodeOrdinary(text);
            var tokenArray = ToInt32Array(tokens);

            Assert.Equal(expected.Length, tokenArray.Length);
            Assert.Equal(expected.TokenIds, tokenArray);
            var idsHash = ParityHashUtilities.HashInt32Sequence(tokenArray);
            Assert.Equal(expected.IdsHash, idsHash);

            var decoded = encoding.Decode(ToUInt32Array(tokenArray));
            decodedTexts.Add(decoded);
            var decodedHash = ParityHashUtilities.HashString(decoded);
            Assert.Equal(expected.DecodedHash, decodedHash);

            batchSnapshots.Add(
                new TiktokenDotnetEncodingSnapshot
                {
                    Length = tokenArray.Length,
                    TokenIds = tokenArray,
                    IdsHash = idsHash,
                    DecodedHash = decodedHash
                });
        }

        var batchDecodedHash = ParityHashUtilities.HashStringSequence(decodedTexts);
        Assert.Equal(batch.DecodedHash, batchDecodedHash);

        return new TiktokenDotnetBenchmarkCaseSnapshot
        {
            ContractId = contractCase.ContractId,
            Length = contractCase.Length,
            Description = contractCase.Description,
            Single = new TiktokenDotnetBenchmarkSingleSnapshot
            {
                Text = single.Text,
                Length = singleTokenArray.Length,
                TokenIds = singleTokenArray,
                IdsHash = singleIdsHash,
                Decoded = singleDecoded,
                DecodedHash = singleDecodedHash
            },
            Batch = new TiktokenDotnetBenchmarkBatchSnapshot
            {
                Count = batch.Count,
                Texts = batch.Texts.ToArray(),
                TextsHash = batch.TextsHash,
                Encodings = batchSnapshots.ToArray(),
                Decoded = decodedTexts.ToArray(),
                DecodedHash = batchDecodedHash
            }
        };
    }

    private static PythonParityBenchmark LoadBenchmark(string modelDirectory)
    {
        var path = Path.Combine(modelDirectory, BenchmarkFileName);
        if (!File.Exists(path))
        {
            throw new XunitException($"Python benchmark file not found: {path}");
        }

        using var stream = File.OpenRead(path);
        var benchmark = JsonSerializer.Deserialize<PythonParityBenchmark>(stream, SerializerOptions);
        if (benchmark is null)
        {
            throw new XunitException($"Unable to deserialize python benchmark from '{path}'.");
        }

        return benchmark;
    }

    private static string ResolveMergeableRanksPath(string modelDirectory, PythonParityMetadata metadata)
    {
        var fileName = metadata.MergeableRanksFile ?? MergeableRanksFileName;
        var path = Path.Combine(modelDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new XunitException($"Mergeable ranks file not found: {path}");
        }

        return path;
    }

    private static string GetBenchmarksRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionPath))
            {
                return Path.Combine(directory.FullName, "tests", "_TestData_Tiktoken");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }

    private static int ToInt32(uint value)
    {
        if (value > int.MaxValue)
        {
            throw new InvalidOperationException($"TikToken ids must fit in 32-bit signed integers. Value: {value}");
        }

        return (int)value;
    }

    private static int[] ToInt32Array(IReadOnlyList<uint> tokens)
    {
        var result = new int[tokens.Count];
        for (var index = 0; index < tokens.Count; index++)
        {
            result[index] = ToInt32(tokens[index]);
        }

        return result;
    }

    private static uint[] ToUInt32Array(IReadOnlyList<int> tokens)
    {
        var result = new uint[tokens.Count];
        for (var index = 0; index < tokens.Count; index++)
        {
            var value = tokens[index];
            if (value < 0)
            {
                throw new InvalidOperationException($"TikToken ids must be non-negative. Value: {value}");
            }

            result[index] = (uint)value;
        }

        return result;
    }

    private sealed class PythonParityBenchmark
    {
    public PythonParityMetadata Metadata { get; set; } = new();

    public List<PythonParityCase> Cases { get; set; } = new();
    }

    private sealed class PythonParityMetadata
    {
    public string Encoding { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public List<string> Aliases { get; set; } = new();

    public string GeneratedAt { get; set; } = string.Empty;

    public string TiktokenVersion { get; set; } = string.Empty;

    public string Pattern { get; set; } = string.Empty;

    public Dictionary<string, int> SpecialTokens { get; set; } = new();

    public int ExplicitVocabularySize { get; set; }
        = 0;

    public int? PythonVocabularySize { get; set; }
        = null;

    public string? MergeableRanksFile { get; set; }
        = MergeableRanksFileName;
    }

    private sealed class PythonParityCase
    {
    public string? ContractId { get; set; }
        = null;

    public string? Length { get; set; }
        = null;

    public string? Description { get; set; }
        = null;

    public PythonParitySingle Single { get; set; } = new();

    public PythonParityBatch Batch { get; set; } = new();
    }

    private sealed class PythonParitySingle
    {
    public string Text { get; set; } = string.Empty;

    public string TextHash { get; set; } = string.Empty;

    public List<int> TokenIds { get; set; } = new();

    public string IdsHash { get; set; } = string.Empty;

    public string DecodedHash { get; set; } = string.Empty;

    public int Length { get; set; }
        = 0;
    }

    private sealed class PythonParityBatch
    {
    public int Count { get; set; }
        = 0;

    public List<string> Texts { get; set; } = new();

    public string TextsHash { get; set; } = string.Empty;

    public List<PythonParityEncoding> Encodings { get; set; } = new();

    public string DecodedHash { get; set; } = string.Empty;
    }

    private sealed class PythonParityEncoding
    {
    public List<int> TokenIds { get; set; } = new();

    public string IdsHash { get; set; } = string.Empty;

    public string DecodedHash { get; set; } = string.Empty;

    public int Length { get; set; }
        = 0;
    }
}
