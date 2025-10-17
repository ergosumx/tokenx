using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class ReplaceDecoderTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        using var decoder = new ReplaceDecoder("_", " ");
        Assert.NotNull(decoder);
        Assert.Equal("_", decoder.Pattern);
        Assert.Equal(" ", decoder.Content);
    }

    [Fact]
    public void Constructor_WithNullPattern_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplaceDecoder(null!, " "));
    }

    [Fact]
    public void Constructor_WithNullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplaceDecoder("_", null!));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new ReplaceDecoder("_", " ");
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new ReplaceDecoder("@@", "##");
        Assert.Equal("@@", decoder.Pattern);
        Assert.Equal("##", decoder.Content);
    }

    [Fact]
    public void MultipleInstances_WithDifferentPatterns_CanCoexist()
    {
        using var decoder1 = new ReplaceDecoder("_", " ");
        using var decoder2 = new ReplaceDecoder("-", "");

        Assert.Equal("_", decoder1.Pattern);
        Assert.Equal("-", decoder2.Pattern);
    }
}
