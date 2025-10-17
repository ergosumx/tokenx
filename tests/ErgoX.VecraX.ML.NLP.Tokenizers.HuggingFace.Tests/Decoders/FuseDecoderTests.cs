using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class FuseDecoderTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        using var decoder = new FuseDecoder();
        Assert.NotNull(decoder);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new FuseDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void MultipleInstances_CanCoexist()
    {
        using var decoder1 = new FuseDecoder();
        using var decoder2 = new FuseDecoder();

        Assert.NotNull(decoder1);
        Assert.NotNull(decoder2);
    }
}
