namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.UnitTests.Tiktoken;

using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;

public sealed class TiktokenEncodingTests : TiktokenTestBase
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
    public void EncodeOrdinary_ReturnsEmptyForNullInput()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);

        var tokens = encoding.EncodeOrdinary(null!);

        Assert.Empty(tokens);
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
    public void Encode_ThrowsWhenAllowedSpecialContainsNull()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);

        Assert.Throws<ArgumentException>(() => encoding.Encode("hi", new[] { "<|test|>", null! }));
    }

    [Fact]
    public void EncodeOrdinary_ThrowsWhenDisposed()
    {
        var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        encoding.Dispose();

        Assert.Throws<ObjectDisposedException>(() => encoding.EncodeOrdinary("hi"));
    }

    [Fact]
    public void Decode_RoundTripsTokens()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = new uint[] { 104, 105, 32, 119, 111, 114, 108, 100 };

        var text = encoding.Decode(tokens);

        Assert.Equal("hi world", text);
    }

    [Fact]
    public void DecodeBytes_ReturnsRawBytes()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);
        var tokens = new uint[] { 65, 66, 67 };

        var bytes = encoding.DecodeBytes(tokens);

        Assert.Equal(new byte[] { 65, 66, 67 }, bytes);
    }

    [Fact]
    public void DecodeBytes_WithEmptyTokensReturnsEmpty()
    {
        using var encoding = TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, SpecialTokens);

        var bytes = encoding.DecodeBytes(Array.Empty<uint>());

        Assert.True(bytes.SequenceEqual(Array.Empty<byte>()));
    }

    [Fact]
    public void Create_WithExplicitVocabularyMismatch_Throws()
    {
        var ex = Assert.Throws<TiktokenInteropException>(
            () => TiktokenEncoding.Create(
                "unit-test",
                "(?s).",
                MergeableRanks,
                SpecialTokens,
                explicitVocabularySize: 1));

        Assert.Contains("explicit vocabulary size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ThrowsWhenSpecialTokenHasNegativeId()
    {
        var specials = new Dictionary<string, int>
        {
            ["<|neg|>"] = -1,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TiktokenEncoding.Create("unit-test", "(?s).", MergeableRanks, specials));
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
