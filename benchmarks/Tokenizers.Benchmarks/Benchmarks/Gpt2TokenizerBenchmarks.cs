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
[BenchmarkCategory("GPT-2", "BPE")]
[MinIterationTime(200)]
public class Gpt2TokenizerBenchmarks : IDisposable
{
    private HuggingFaceTokenizer _huggingFaceTokenizer = null!;
    private BpeTokenizer _microsoftTokenizer = null!;
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
        var assets = ModelAssets.EnsureGpt2();
    _huggingFaceTokenizer = HuggingFaceTokenizer.FromFile(assets.TokenizerJsonPath);
        _microsoftTokenizer = BpeTokenizer.Create(assets.VocabPath!, assets.MergesPath!);
        PrepareCachedTokens();
    }

    [IterationSetup]
    public void IterationSetup() => PrepareCachedTokens();

    [GlobalCleanup]
    public void GlobalCleanup() => Dispose();

    [Benchmark]
    public int HuggingFace_EncodeSingle() => _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: false).Length;

    [Benchmark]
    public int Microsoft_EncodeSingle() => _microsoftTokenizer.EncodeToIds(Case.Text, false, false).Count;

    [Benchmark]
    public int HuggingFace_EncodeBatch()
        => _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: false).Sum(result => result.Length);

    [Benchmark]
    public int Microsoft_EncodeBatch()
    {
        var total = 0;
        foreach (var text in Case.Batch)
        {
            total += _microsoftTokenizer.EncodeToIds(text, false, false).Count;
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
        var decoded = _huggingFaceTokenizer.DecodeBatch(_huggingFaceBatchTokens, skipSpecialTokens: true);
        return decoded.Sum(text => text.Length);
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
        _huggingFaceSingleTokens = _huggingFaceTokenizer.Encode(Case.Text, addSpecialTokens: false).Ids.ToArray();
    _microsoftSingleTokens = _microsoftTokenizer.EncodeToIds(Case.Text, false, false).ToArray();
        _huggingFaceBatchTokens = _huggingFaceTokenizer.EncodeBatch(Case.Batch, addSpecialTokens: false)
            .Select(result => result.Ids.ToArray())
            .ToArray();
        _microsoftBatchTokens = Case.Batch
            .Select(text => _microsoftTokenizer.EncodeToIds(text, false, false).ToArray())
            .ToArray();
    }
}
