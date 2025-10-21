namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Unit;

using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class EncodeOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var options = new EncodeOptions();

        Assert.False(options.AddBos);
        Assert.False(options.AddEos);
        Assert.False(options.Reverse);
        Assert.False(options.EmitUnknownPiece);
        Assert.False(options.EnableSampling);
        Assert.Equal(-1, options.NBestSize);
        Assert.Equal(0f, options.Alpha);
    }

    [Fact]
    public void Properties_AreMutable()
    {
        var options = new EncodeOptions
        {
            AddBos = true,
            AddEos = true,
            Reverse = true,
            EmitUnknownPiece = true,
            EnableSampling = true,
            NBestSize = 42,
            Alpha = 0.75f,
        };

        Assert.True(options.AddBos);
        Assert.True(options.AddEos);
        Assert.True(options.Reverse);
        Assert.True(options.EmitUnknownPiece);
        Assert.True(options.EnableSampling);
        Assert.Equal(42, options.NBestSize);
        Assert.Equal(0.75f, options.Alpha);
    }
}
