using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class ByteFallbackDecoderTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        using var decoder = new ByteFallbackDecoder();
        Assert.NotNull(decoder);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new ByteFallbackDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void MultipleInstances_CanCoexist()
    {
        using var decoder1 = new ByteFallbackDecoder();
        using var decoder2 = new ByteFallbackDecoder();

        Assert.NotNull(decoder1);
        Assert.NotNull(decoder2);
    }
}
