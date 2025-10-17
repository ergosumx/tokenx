using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class MetaspaceDecoderTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        using var decoder = new MetaspaceDecoder();
        Assert.NotNull(decoder);
        Assert.Equal('▁', decoder.Replacement);
        Assert.Equal(PrependScheme.Always, decoder.PrependScheme);
        Assert.True(decoder.Split);
    }

    [Fact]
    public void Constructor_WithCustomParameters_CreatesInstance()
    {
        using var decoder = new MetaspaceDecoder('_', PrependScheme.First, false);
        Assert.NotNull(decoder);
        Assert.Equal('_', decoder.Replacement);
        Assert.Equal(PrependScheme.First, decoder.PrependScheme);
        Assert.False(decoder.Split);
    }

    [Fact]
    public void PrependScheme_Always_WorksCorrectly()
    {
        using var decoder = new MetaspaceDecoder('▁', PrependScheme.Always, true);
        Assert.Equal(PrependScheme.Always, decoder.PrependScheme);
    }

    [Fact]
    public void PrependScheme_First_WorksCorrectly()
    {
        using var decoder = new MetaspaceDecoder('▁', PrependScheme.First, true);
        Assert.Equal(PrependScheme.First, decoder.PrependScheme);
    }

    [Fact]
    public void PrependScheme_Never_WorksCorrectly()
    {
        using var decoder = new MetaspaceDecoder('▁', PrependScheme.Never, true);
        Assert.Equal(PrependScheme.Never, decoder.PrependScheme);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new MetaspaceDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new MetaspaceDecoder(' ', PrependScheme.Never, false);
        Assert.Equal(' ', decoder.Replacement);
        Assert.Equal(PrependScheme.Never, decoder.PrependScheme);
        Assert.False(decoder.Split);
    }

    [Fact]
    public void MultipleInstances_WithDifferentSettings_CanCoexist()
    {
        using var decoder1 = new MetaspaceDecoder('▁', PrependScheme.Always, true);
        using var decoder2 = new MetaspaceDecoder('_', PrependScheme.First, false);
        using var decoder3 = new MetaspaceDecoder(' ', PrependScheme.Never, true);

        Assert.Equal('▁', decoder1.Replacement);
        Assert.Equal('_', decoder2.Replacement);
        Assert.Equal(' ', decoder3.Replacement);
    }

    [Fact]
    public void SplitTrue_RetainsProperty()
    {
        using var decoder = new MetaspaceDecoder('▁', PrependScheme.Always, true);
        Assert.True(decoder.Split);
    }

    [Fact]
    public void SplitFalse_RetainsProperty()
    {
        using var decoder = new MetaspaceDecoder('▁', PrependScheme.Always, false);
        Assert.False(decoder.Split);
    }

    [Fact]
    public void UnicodeReplacement_IsSupported()
    {
        // Using a simple Unicode character that fits in a single char
        using var decoder = new MetaspaceDecoder('→', PrependScheme.Always, true);
        Assert.Equal('→', decoder.Replacement);
    }

    [Fact]
    public void SpecialCharacterReplacement_IsSupported()
    {
        using var decoder = new MetaspaceDecoder('@', PrependScheme.First, true);
        Assert.Equal('@', decoder.Replacement);
    }
}
