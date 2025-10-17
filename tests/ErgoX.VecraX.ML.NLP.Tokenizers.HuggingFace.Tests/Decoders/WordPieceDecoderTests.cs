using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class WordPieceDecoderTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        using var decoder = new WordPieceDecoder();
        Assert.NotNull(decoder);
        Assert.Equal("##", decoder.Prefix);
        Assert.True(decoder.Cleanup);
    }

    [Fact]
    public void Constructor_WithCustomParameters_CreatesInstance()
    {
        using var decoder = new WordPieceDecoder("__", false);
        Assert.NotNull(decoder);
        Assert.Equal("__", decoder.Prefix);
        Assert.False(decoder.Cleanup);
    }

    [Fact]
    public void Constructor_WithNullPrefix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WordPieceDecoder(null!, true));
    }

    [Fact]
    public void Constructor_WithEmptyPrefix_CreatesInstance()
    {
        using var decoder = new WordPieceDecoder("", true);
        Assert.NotNull(decoder);
        Assert.Equal("", decoder.Prefix);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new WordPieceDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new WordPieceDecoder("@@", false);
        Assert.Equal("@@", decoder.Prefix);
        Assert.False(decoder.Cleanup);
    }

    [Fact]
    public void MultipleInstances_WithDifferentSettings_CanCoexist()
    {
        using var decoder1 = new WordPieceDecoder("##", true);
        using var decoder2 = new WordPieceDecoder("__", false);
        using var decoder3 = new WordPieceDecoder("", true);

        Assert.Equal("##", decoder1.Prefix);
        Assert.Equal("__", decoder2.Prefix);
        Assert.Equal("", decoder3.Prefix);
    }

    [Fact]
    public void UnicodePrefix_IsSupported()
    {
        using var decoder = new WordPieceDecoder("▁▁", true);
        Assert.Equal("▁▁", decoder.Prefix);
    }

    [Fact]
    public void LongPrefix_IsSupported()
    {
        var longPrefix = new string('#', 50);
        using var decoder = new WordPieceDecoder(longPrefix, true);
        Assert.Equal(longPrefix, decoder.Prefix);
    }

    [Fact]
    public void CleanupTrue_RetainsProperty()
    {
        using var decoder = new WordPieceDecoder("##", true);
        Assert.True(decoder.Cleanup);
    }

    [Fact]
    public void CleanupFalse_RetainsProperty()
    {
        using var decoder = new WordPieceDecoder("##", false);
        Assert.False(decoder.Cleanup);
    }
}
