using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class ByteLevelDecoderTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        using var decoder = new ByteLevelDecoder();
        Assert.NotNull(decoder);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var decoder = new ByteLevelDecoder();
        decoder.Dispose();
        Assert.True(true); // Verify no exception thrown
        decoder.Dispose(); // Should not throw
    }

    [Fact]
    public void MultipleInstances_CanCoexist()
    {
        using var decoder1 = new ByteLevelDecoder();
        using var decoder2 = new ByteLevelDecoder();
        using var decoder3 = new ByteLevelDecoder();

        Assert.NotNull(decoder1);
        Assert.NotNull(decoder2);
        Assert.NotNull(decoder3);
    }

    [Fact]
    public void SequentialCreateAndDispose_WorksCorrectly()
    {
        for (int i = 0; i < 10; i++)
        {
            using var decoder = new ByteLevelDecoder();
            Assert.NotNull(decoder);
        }
    }
}
