namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.UnitTests.Generation;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class GenerationPlannerContractsUnitTests : HuggingFaceTestBase
{
    [Fact]
    public void LogitsBinding_ValidatesCategory()
    {
        Assert.Throws<ArgumentException>(() => new LogitsBinding(null!, "temperature", 0.5));
        Assert.Throws<ArgumentException>(() => new LogitsBinding("", "temperature", 0.5));
        Assert.Throws<ArgumentException>(() => new LogitsBinding("   ", "temperature", 0.5));
    }

    [Fact]
    public void LogitsBinding_ValidatesKind()
    {
        Assert.Throws<ArgumentException>(() => new LogitsBinding("warper", null!, 0.5));
        Assert.Throws<ArgumentException>(() => new LogitsBinding("warper", "", 0.5));
        Assert.Throws<ArgumentException>(() => new LogitsBinding("warper", "   ", 0.5));
    }

    [Fact]
    public void LogitsBinding_ValidatesValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogitsBinding("warper", "temperature", double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogitsBinding("warper", "temperature", double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogitsBinding("warper", "temperature", double.NegativeInfinity));
    }

    [Fact]
    public void LogitsBinding_AcceptsValidValues()
    {
        var binding1 = new LogitsBinding("warper", "temperature", 0.0);
        Assert.Equal(0.0, binding1.Value);

        var binding2 = new LogitsBinding("warper", "temperature", 1.5);
        Assert.Equal(1.5, binding2.Value);

        var binding3 = new LogitsBinding("warper", "temperature", -1.0);
        Assert.Equal(-1.0, binding3.Value);
    }

    [Fact]
    public void LogitsBinding_IdentifiesWarper()
    {
        var binding = new LogitsBinding("warper", "temperature", 0.7);
        Assert.True(binding.IsWarper);
        Assert.False(binding.IsProcessor);
    }

    [Fact]
    public void LogitsBinding_IdentifiesProcessor()
    {
        var binding = new LogitsBinding("processor", "repetition_penalty", 1.2);
        Assert.False(binding.IsWarper);
        Assert.True(binding.IsProcessor);
    }

    [Fact]
    public void LogitsBinding_HandlesOtherCategory()
    {
        var binding = new LogitsBinding("other", "custom", 0.5);
        Assert.False(binding.IsWarper);
        Assert.False(binding.IsProcessor);
    }

    [Fact]
    public void StoppingCriterion_ValidatesKind()
    {
        Assert.Throws<ArgumentException>(() => new StoppingCriterion(null!, 100, null));
        Assert.Throws<ArgumentException>(() => new StoppingCriterion("", 100, null));
        Assert.Throws<ArgumentException>(() => new StoppingCriterion("   ", 100, null));
    }

    [Fact]
    public void StoppingCriterion_AcceptsNullValue()
    {
        var criterion = new StoppingCriterion("max_new_tokens", null, null);
        Assert.Null(criterion.Value);
    }

    [Fact]
    public void StoppingCriterion_AcceptsNullSequences()
    {
        var criterion = new StoppingCriterion("max_new_tokens", 100, null);
        Assert.Null(criterion.Sequences);
    }

    [Fact]
    public void StoppingCriterion_IdentifiesMaxNewTokens()
    {
        var criterion = new StoppingCriterion("max_new_tokens", 256, null);
        Assert.True(criterion.IsMaxNewTokens);
        Assert.False(criterion.IsStopSequences);
        Assert.Equal(256, criterion.Value);
    }

    [Fact]
    public void StoppingCriterion_IdentifiesStopSequences()
    {
        var sequences = new[] { "END", "STOP" };
        var criterion = new StoppingCriterion("stop_sequences", null, sequences);
        Assert.True(criterion.IsStopSequences);
        Assert.False(criterion.IsMaxNewTokens);
        Assert.NotNull(criterion.Sequences);
        Assert.Equal(2, criterion.Sequences.Count);
    }

    [Fact]
    public void StoppingCriterion_HandlesOtherKind()
    {
        var criterion = new StoppingCriterion("custom", 10, null);
        Assert.False(criterion.IsMaxNewTokens);
        Assert.False(criterion.IsStopSequences);
    }

    [Fact]
    public void StoppingCriterion_AcceptsValidCombinations()
    {
        var criterion1 = new StoppingCriterion("max_new_tokens", 100, null);
        Assert.Equal("max_new_tokens", criterion1.Kind);
        Assert.Equal(100, criterion1.Value);
        Assert.Null(criterion1.Sequences);

        var criterion2 = new StoppingCriterion("stop_sequences", null, new[] { "STOP" });
        Assert.Equal("stop_sequences", criterion2.Kind);
        Assert.Null(criterion2.Value);
        Assert.NotNull(criterion2.Sequences);
        Assert.Single(criterion2.Sequences);

        var criterion3 = new StoppingCriterion("custom", 50, new[] { "A", "B" });
        Assert.Equal("custom", criterion3.Kind);
        Assert.Equal(50, criterion3.Value);
        Assert.NotNull(criterion3.Sequences);
        Assert.Equal(2, criterion3.Sequences.Count);
    }
}
