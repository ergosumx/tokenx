namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tests.UnitTests.Tiktoken;

using System;
using System.IO;
using System.Text;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;
using Xunit;

public sealed class TiktokenBpeLoaderTests : TiktokenTestBase
{
    [Fact]
    public void LoadStream_ParsesValidEntries()
    {
        using var stream = CreateStream("QQ== 0\nQg== 1\n\n");

        var ranks = TiktokenBpeLoader.Load(stream);

        Assert.Equal(2, ranks.Count);
        Assert.Equal(new byte[] { 0x41 }, ranks[0].Token.ToArray());
        Assert.Equal(0, ranks[0].Rank);
        Assert.Equal(new byte[] { 0x42 }, ranks[1].Token.ToArray());
        Assert.Equal(1, ranks[1].Rank);
    }

    [Fact]
    public void LoadFile_UsesFileSystem()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "\nQw== 2\n");

            var ranks = TiktokenBpeLoader.Load(path);

            Assert.Single(ranks);
            Assert.Equal(2, ranks[0].Rank);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadStream_ThrowsForInvalidBase64()
    {
        using var stream = CreateStream("not-base64 0\n");

        var ex = Assert.Throws<FormatException>(() => TiktokenBpeLoader.Load(stream));

        Assert.Contains("Invalid base64 token", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadStream_ThrowsForInvalidRank()
    {
        using var stream = CreateStream("QQ== not-a-number\n");

        var ex = Assert.Throws<FormatException>(() => TiktokenBpeLoader.Load(stream));

        Assert.Contains("Invalid rank value", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadStream_ThrowsForMalformedLine()
    {
        using var stream = CreateStream("QQ==\n");

        var ex = Assert.Throws<FormatException>(() => TiktokenBpeLoader.Load(stream));

        Assert.Contains("Malformed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadStream_ThrowsForNullStream()
    {
        Assert.Throws<ArgumentNullException>(() => TiktokenBpeLoader.Load((Stream)null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void LoadFile_ThrowsForInvalidPath(string? path)
    {
        Assert.Throws<ArgumentException>(() => TiktokenBpeLoader.Load(path!));
    }

    private static MemoryStream CreateStream(string content)
    {
        var bytes = Encoding.ASCII.GetBytes(content);
        return new MemoryStream(bytes, writable: false);
    }
}
