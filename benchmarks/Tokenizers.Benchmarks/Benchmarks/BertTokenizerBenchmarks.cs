namespace Tokenizers.Benchmarks.Benchmarks;

using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.ML.Tokenizers;
using Tokenizers.Benchmarks.Assets;
using Tokenizers.Benchmarks.Infrastructure;
using HuggingFaceTokenizer = ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tokenizer;

[Config(typeof(TokenizerBenchmarkConfig))]
[MemoryDiagnoser]
[BenchmarkCategory("BERT", "WordPiece")]
[MinIterationTime(200)]
public class BertTokenizerBenchmarks : IDisposable
{
    private HuggingFaceTokenizer _huggingFaceTokenizer = null!;
    private WordPieceTokenizer _microsoftTokenizer = null!;
    private int[] _huggingFaceSingleTokens = Array.Empty<int>();
    private int[] _microsoftSingleTokens = Array.Empty<int>();
    private int[][] _huggingFaceBatchTokens = Array.Empty<int[]>();
    private int[][] _microsoftBatchTokens = Array.Empty<int[]>();

    [ParamsSource(nameof(Cases))]
    public BenchmarkCase Case { get; set; } = null!;

    public static IEnumerable<BenchmarkCase> Cases => TextCorpus.GetCases();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var assets = ModelAssets.EnsureBertBaseUncased();
    _huggingFaceTokenizer = HuggingFaceTokenizer.FromFile(assets.TokenizerJsonPath);

        var options = new WordPieceOptions
        {
            UnknownToken = "[UNK]",
            ContinuingSubwordPrefix = "##",
            MaxInputCharsPerWord = 200
        };

        _microsoftTokenizer = WordPieceTokenizer.Create(assets.VocabPath!, options);
        PrepareCachedTokens();
    }

    [IterationSetup]
    public void IterationSetup() => PrepareCachedTokens();

    [GlobalCleanup]
    public void GlobalCleanup() => Dispose();

    [Benchmark]
    public int HuggingFace_EncodeSingle() => _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: true).Length;

    [Benchmark]
    public int Microsoft_EncodeSingle() => _microsoftTokenizer.EncodeToIds(Case.Text, true, true).Count;

    [Benchmark]
    public int HuggingFace_EncodeBatch()
        => _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: true).Sum(result => result.Length);

    [Benchmark]
    public int Microsoft_EncodeBatch()
    {
        var total = 0;
        foreach (var text in Case.Batch)
        {
            total += _microsoftTokenizer.EncodeToIds(text, true, true).Count;
        }

        return total;
    }

    [Benchmark]
    public int HuggingFace_DecodeSingle() => _huggingFaceTokenizer.Decode(_huggingFaceSingleTokens, skipSpecialTokens: true).Length;

    [Benchmark]
    public int Microsoft_DecodeSingle() => _microsoftTokenizer.Decode(_microsoftSingleTokens, true).Length;

    [Benchmark]
    public int HuggingFace_DecodeBatch()
    {
        var total = 0;
        foreach (var tokens in _huggingFaceBatchTokens)
        {
            total += _huggingFaceTokenizer.Decode(tokens, skipSpecialTokens: true).Length;
        }

        return total;
    }

    [Benchmark]
    public int Microsoft_DecodeBatch()
    {
        var total = 0;
        foreach (var tokens in _microsoftBatchTokens)
        {
            total += _microsoftTokenizer.Decode(tokens, true).Length;
        }

        return total;
    }

    public void Dispose()
    {
        _huggingFaceTokenizer?.Dispose();
    }

    private void PrepareCachedTokens()
    {
        _huggingFaceSingleTokens = _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: true).Ids.ToArray();
    _microsoftSingleTokens = _microsoftTokenizer.EncodeToIds(Case.Text, true, true).ToArray();
        _huggingFaceBatchTokens = _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: true)
            .Select(result => result.Ids.ToArray())
            .ToArray();
        _microsoftBatchTokens = Case.Batch
            .Select(text => _microsoftTokenizer.EncodeToIds(text, true, true).ToArray())
            .ToArray();
    }
}
