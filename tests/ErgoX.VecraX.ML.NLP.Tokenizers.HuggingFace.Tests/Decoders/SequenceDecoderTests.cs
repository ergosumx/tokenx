using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Decoders;

public class SequenceDecoderTests
{
    [Fact]
    public void Constructor_WithValidDecoders_CreatesInstance()
    {
        using var byteFallback = new ByteFallbackDecoder();
        using var metaspace = new MetaspaceDecoder();

        using var sequence = new SequenceDecoder(byteFallback, metaspace);

        Assert.NotEqual(IntPtr.Zero, sequence.Handle);
    }

    [Fact]
    public void Constructor_WithNullEnumerable_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SequenceDecoder((IEnumerable<IDecoder>)null!));
    }

    [Fact]
    public void Constructor_WithNullEntry_Throws()
    {
        using var byteFallback = new ByteFallbackDecoder();
        IDecoder?[] decoders = { byteFallback, null };

        Assert.Throws<ArgumentException>(() => new SequenceDecoder(decoders!));
    }

    [Fact]
    public void Constructor_WithEmptyEnumerable_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SequenceDecoder(Array.Empty<IDecoder>()));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        using var byteFallback = new ByteFallbackDecoder();
        using var metaspace = new MetaspaceDecoder();

        var sequence = new SequenceDecoder(byteFallback, metaspace);
        sequence.Dispose();
        sequence.Dispose();
    }
}
