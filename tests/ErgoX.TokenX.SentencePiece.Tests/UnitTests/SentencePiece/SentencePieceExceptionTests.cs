namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Unit;

using System;
using System.Reflection;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Exceptions;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class SentencePieceExceptionTests
{
    [Fact]
    public void Constructor_AssignsStatusCode()
    {
        var exception = new SentencePieceException("message", SentencePieceStatusCode.Internal);

        Assert.Equal("message", exception.Message);
        Assert.Equal(SentencePieceStatusCode.Internal, exception.StatusCode);
    }

    [Theory]
    [InlineData(0, SentencePieceStatusCode.Ok)]
    [InlineData(3, SentencePieceStatusCode.InvalidArgument)]
    [InlineData(5, SentencePieceStatusCode.NotFound)]
    public void FromNative_MapsValues(int nativeValue, SentencePieceStatusCode expected)
    {
        var actual = InvokeFromNative(nativeValue);
        Assert.Equal(expected, actual);
    }

    private static SentencePieceStatusCode InvokeFromNative(int nativeValue)
    {
        var method = typeof(SentencePieceException).GetMethod("FromNative", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var nativeEnumType = typeof(SentencePieceProcessor).Assembly.GetType("ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop.NativeMethods+SpcStatusCode", throwOnError: true) ?? throw new InvalidOperationException("Native status enum type could not be located.");
        var nativeEnumValue = Enum.ToObject(nativeEnumType, nativeValue);

        return (SentencePieceStatusCode)method!.Invoke(null, new[] { nativeEnumValue })!;
    }
}
