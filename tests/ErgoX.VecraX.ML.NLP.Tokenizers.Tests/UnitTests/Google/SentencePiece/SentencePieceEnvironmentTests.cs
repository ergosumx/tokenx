namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Unit;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class SentencePieceEnvironmentTests
{
    [Fact]
    public void SetRandomGeneratorSeed_DoesNotThrow()
    {
        var exception = Record.Exception(() => SentencePieceEnvironment.SetRandomGeneratorSeed(123u));
        Assert.Null(exception);
    }

    [Fact]
    public void SetMinLogLevel_DoesNotThrow()
    {
        var exception = Record.Exception(() => SentencePieceEnvironment.SetMinLogLevel(2));
        Assert.Null(exception);
    }

    [Fact]
    public void SetDataDirectory_RequiresPath()
    {
        Assert.Throws<ArgumentException>(() => SentencePieceEnvironment.SetDataDirectory(""));
        Assert.Throws<ArgumentException>(() => SentencePieceEnvironment.SetDataDirectory("   "));
    }
}
