namespace ErgoX.TokenX.Tests.UnitTests.Tiktoken;

using System;
using ErgoX.TokenX.Tests;
using ErgoX.TokenX.Tiktoken;
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

