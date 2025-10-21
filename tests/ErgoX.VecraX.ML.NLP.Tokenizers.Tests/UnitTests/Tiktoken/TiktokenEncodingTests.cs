namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.UnitTests.Tiktoken;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;

public sealed class TiktokenEncodingTests
{
    private static readonly IReadOnlyList<TiktokenMergeableRank> MergeableRanks = BuildMergeableRanks();

    private static readonly IReadOnlyDictionary<string, int> SpecialTokens = new Dictionary<string, int>
    {
        ["<|test|>"] = 256,
    };

    [Fact]
    public void EncodeOrdinary_ReturnsExpectedTokens()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = encoding.EncodeOrdinary("hi");

        Assert.Equal(new uint[] { 104, 105 }, tokens);
    }

    [Fact]
    public void Encode_AllowsExplicitSpecialTokens()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = encoding.Encode("hi<|test|>", new[] { "<|test|>" });

        Assert.Equal(new uint[] { 104, 105, 256 }, tokens);
    }

    [Fact]
    public void Encode_TreatsDisallowedSpecialAsText()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = encoding.Encode("<|test|>");

        Assert.Equal(new uint[] { 60, 124, 116, 101, 115, 116, 124, 62 }, tokens);
    }

    [Fact]
    public void Decode_RoundTripsTokens()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = new uint[] { 104, 105, 32, 119, 111, 114, 108, 100 };

        var text = encoding.Decode(tokens);

        Assert.Equal("hi world", text);
    }

    private static IReadOnlyList<TiktokenMergeableRank> BuildMergeableRanks()
    {
        var ranks = new List<TiktokenMergeableRank>(256);

        for (var value = 0; value < 256; value++)
        {
            ranks.Add(new TiktokenMergeableRank(new byte[] { (byte)value }, value));
        }

        return ranks;
    }
}
