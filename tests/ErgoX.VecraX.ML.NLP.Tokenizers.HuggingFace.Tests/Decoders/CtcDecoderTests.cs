using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class CtcDecoderTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        using var decoder = new CtcDecoder();
        Assert.NotNull(decoder);
        Assert.Equal("<pad>", decoder.PadToken);
        Assert.Equal("|", decoder.WordDelimiterToken);
        Assert.True(decoder.Cleanup);
    }

    [Fact]
    public void Constructor_WithCustomParameters_CreatesInstance()
    {
        using var decoder = new CtcDecoder("[PAD]", "▁", false);
        Assert.NotNull(decoder);
        Assert.Equal("[PAD]", decoder.PadToken);
        Assert.Equal("▁", decoder.WordDelimiterToken);
        Assert.False(decoder.Cleanup);
    }

    [Fact]
    public void Constructor_WithNullPadToken_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CtcDecoder(null!, "|", true));
    }

    [Fact]
    public void Constructor_WithNullWordDelimiter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CtcDecoder("<pad>", null!, true));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new CtcDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new CtcDecoder("[BLANK]", "_", false);
        Assert.Equal("[BLANK]", decoder.PadToken);
        Assert.Equal("_", decoder.WordDelimiterToken);
        Assert.False(decoder.Cleanup);
    }

    [Fact]
    public void MultipleInstances_WithDifferentSettings_CanCoexist()
    {
        using var decoder1 = new CtcDecoder("<pad>", "|", true);
        using var decoder2 = new CtcDecoder("[PAD]", "_", false);

        Assert.Equal("<pad>", decoder1.PadToken);
        Assert.Equal("[PAD]", decoder2.PadToken);
    }
}
