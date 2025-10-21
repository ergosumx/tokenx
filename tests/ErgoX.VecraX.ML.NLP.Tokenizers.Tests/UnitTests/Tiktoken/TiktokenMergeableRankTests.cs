namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.UnitTests.Tiktoken;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;

public sealed class TiktokenMergeableRankTests : TiktokenTestBase
{
    [Fact]
    public void Constructor_PopulatesProperties()
    {
        var bytes = new byte[] { 1, 2, 3 };

        var rank = new TiktokenMergeableRank(bytes, 42);

        Assert.Equal(42, rank.Rank);
        Assert.Equal(bytes, rank.Token.ToArray());
    }

    [Fact]
    public void Constructor_ThrowsWhenRankNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TiktokenMergeableRank(ReadOnlyMemory<byte>.Empty, -1));
    }
}
