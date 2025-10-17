using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class StripDecoderTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        using var decoder = new StripDecoder();
        Assert.NotNull(decoder);
        Assert.Equal(' ', decoder.Content);
        Assert.Equal((nuint)0, decoder.Left);
        Assert.Equal((nuint)0, decoder.Right);
    }

    [Fact]
    public void Constructor_WithCustomParameters_CreatesInstance()
    {
        using var decoder = new StripDecoder('_', 2, 1);
        Assert.NotNull(decoder);
        Assert.Equal('_', decoder.Content);
        Assert.Equal((nuint)2, decoder.Left);
        Assert.Equal((nuint)1, decoder.Right);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new StripDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void Properties_RetainCorrectValues()
    {
        using var decoder = new StripDecoder('#', 3, 2);
        Assert.Equal('#', decoder.Content);
        Assert.Equal((nuint)3, decoder.Left);
        Assert.Equal((nuint)2, decoder.Right);
    }

    [Fact]
    public void MultipleInstances_WithDifferentSettings_CanCoexist()
    {
        using var decoder1 = new StripDecoder(' ', 1, 0);
        using var decoder2 = new StripDecoder('_', 0, 2);

        Assert.Equal(' ', decoder1.Content);
        Assert.Equal('_', decoder2.Content);
    }
}
