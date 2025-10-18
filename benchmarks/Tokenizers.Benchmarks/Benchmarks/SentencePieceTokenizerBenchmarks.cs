namespace Tokenizers.Benchmarks.Benchmarks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.ML.Tokenizers;
using Sentencepiece;
using Tokenizers.Benchmarks.Assets;
using Tokenizers.Benchmarks.Infrastructure;
using HuggingFaceTokenizer = ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tokenizer;

[Config(typeof(TokenizerBenchmarkConfig))]
[MemoryDiagnoser]
[BenchmarkCategory("T5", "SentencePiece")]
public class SentencePieceTokenizerBenchmarks : IDisposable
{
    private HuggingFaceTokenizer _huggingFaceTokenizer = null!;
    private SentencePieceTokenizer _microsoftTokenizer = null!;
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
        var assets = ModelAssets.EnsureT5Small();
    _huggingFaceTokenizer = HuggingFaceTokenizer.FromFile(assets.TokenizerJsonPath);

        var modelBytes = File.ReadAllBytes(assets.SentencePieceModelPath!);
        var proto = ModelProto.Parser.ParseFrom(modelBytes);
        _microsoftTokenizer = new SentencePieceTokenizer(proto);

        PrepareCachedTokens();
    }

    [IterationSetup]
    public void IterationSetup() => PrepareCachedTokens();

    [GlobalCleanup]
    public void GlobalCleanup() => Dispose();

    [Benchmark(Baseline = true)]
    public int HuggingFace_EncodeSingle() => _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: true).Length;

    [Benchmark]
    public int Microsoft_EncodeSingle() => _microsoftTokenizer.EncodeToIds(Case.Text, addBeginningOfSentence: true, addEndOfSentence: true, addDummyPrefix: false, escapeWhitespace: false).Count;

    [Benchmark(Baseline = true)]
    public int HuggingFace_EncodeBatch()
        => _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: true).Sum(result => result.Length);

    [Benchmark]
    public int Microsoft_EncodeBatch()
    {
        var total = 0;
        foreach (var text in Case.Batch)
        {
            total += _microsoftTokenizer.EncodeToIds(text, addBeginningOfSentence: true, addEndOfSentence: true, addDummyPrefix: false, escapeWhitespace: false).Count;
        }

        return total;
    }

    [Benchmark(Baseline = true)]
    public int HuggingFace_DecodeSingle() => _huggingFaceTokenizer.Decode(_huggingFaceSingleTokens, skipSpecialTokens: true).Length;

    [Benchmark]
    public int Microsoft_DecodeSingle() => _microsoftTokenizer.Decode(_microsoftSingleTokens, skipSpecialTokens: true).Length;

    [Benchmark(Baseline = true)]
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
            total += _microsoftTokenizer.Decode(tokens, skipSpecialTokens: true).Length;
        }

        return total;
    }

    public void Dispose()
    {
        _huggingFaceTokenizer?.Dispose();
        (_microsoftTokenizer as IDisposable)?.Dispose();
    }

    private void PrepareCachedTokens()
    {
        _huggingFaceSingleTokens = _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: true).Ids.ToArray();
        _microsoftSingleTokens = _microsoftTokenizer.EncodeToIds(Case.Text, addBeginningOfSentence: true, addEndOfSentence: true, addDummyPrefix: false, escapeWhitespace: false).ToArray();
        _huggingFaceBatchTokens = _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: true)
            .Select(result => result.Ids.ToArray())
            .ToArray();
        _microsoftBatchTokens = Case.Batch
            .Select(text => _microsoftTokenizer.EncodeToIds(text, addBeginningOfSentence: true, addEndOfSentence: true, addDummyPrefix: false, escapeWhitespace: false).ToArray())
            .ToArray();
    }
}
