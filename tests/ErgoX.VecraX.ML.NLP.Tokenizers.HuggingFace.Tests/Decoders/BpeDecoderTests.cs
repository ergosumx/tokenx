using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class BpeDecoderTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        using var decoder = new BpeDecoder();
        Assert.NotNull(decoder);
        Assert.Equal("</w>", decoder.Suffix);
    }

    [Fact]
    public void Constructor_WithCustomSuffix_CreatesInstance()
    {
        using var decoder = new BpeDecoder("@@");
        Assert.NotNull(decoder);
        Assert.Equal("@@", decoder.Suffix);
    }

    [Fact]
    public void Constructor_WithNullSuffix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BpeDecoder(null!));
    }

    [Fact]
    public void Constructor_WithEmptySuffix_CreatesInstance()
    {
        using var decoder = new BpeDecoder("");
        Assert.NotNull(decoder);
        Assert.Equal("", decoder.Suffix);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new BpeDecoder();
        decoder.Dispose();
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new BpeDecoder("_EOW_");
        Assert.Equal("_EOW_", decoder.Suffix);
    }

    [Fact]
    public void MultipleInstances_CanCoexist()
    {
        using var decoder1 = new BpeDecoder("</w>");
        using var decoder2 = new BpeDecoder("@@");
        using var decoder3 = new BpeDecoder("_");

        Assert.Equal("</w>", decoder1.Suffix);
        Assert.Equal("@@", decoder2.Suffix);
        Assert.Equal("_", decoder3.Suffix);
    }

    [Fact]
    public void SequentialCreateAndDispose_WorksCorrectly()
    {
        for (int i = 0; i < 10; i++)
        {
            using var decoder = new BpeDecoder("_" + i);
            Assert.NotNull(decoder);
        }
    }

    [Fact]
    public void UnicodeSuffix_IsSupported()
    {
        using var decoder = new BpeDecoder("▁");
        Assert.Equal("▁", decoder.Suffix);
    }

    [Fact]
    public void LongSuffix_IsSupported()
    {
        var longSuffix = new string('x', 100);
        using var decoder = new BpeDecoder(longSuffix);
        Assert.Equal(longSuffix, decoder.Suffix);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_CreatesInstance()
    {
        using var decoder = new BpeDecoder("!@#$%");
        Assert.Equal("!@#$%", decoder.Suffix);
    }

    [Fact]
    public void DefaultConstructor_UsesStandardSuffix()
    {
        using var decoder1 = new BpeDecoder();
        using var decoder2 = new BpeDecoder("</w>");

        Assert.Equal(decoder2.Suffix, decoder1.Suffix);
    }
}
