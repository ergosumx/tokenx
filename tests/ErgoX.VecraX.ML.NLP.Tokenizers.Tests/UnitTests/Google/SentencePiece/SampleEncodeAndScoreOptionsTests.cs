namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests.Unit;

using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class SampleEncodeAndScoreOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var options = new SampleEncodeAndScoreOptions();

        Assert.False(options.AddBos);
        Assert.False(options.AddEos);
        Assert.False(options.Reverse);
        Assert.False(options.EmitUnknownPiece);
        Assert.Equal(1, options.NumSamples);
        Assert.Equal(0.1f, options.Alpha);
        Assert.False(options.WithoutReplacement);
        Assert.True(options.IncludeBest);
    }

    [Fact]
    public void Properties_AreMutable()
    {
        var options = new SampleEncodeAndScoreOptions
        {
            AddBos = true,
            AddEos = true,
            Reverse = true,
            EmitUnknownPiece = true,
            NumSamples = 16,
            Alpha = 0.42f,
            WithoutReplacement = true,
            IncludeBest = false,
        };

        Assert.True(options.AddBos);
        Assert.True(options.AddEos);
        Assert.True(options.Reverse);
        Assert.True(options.EmitUnknownPiece);
        Assert.Equal(16, options.NumSamples);
        Assert.Equal(0.42f, options.Alpha);
        Assert.True(options.WithoutReplacement);
        Assert.False(options.IncludeBest);
    }
}
