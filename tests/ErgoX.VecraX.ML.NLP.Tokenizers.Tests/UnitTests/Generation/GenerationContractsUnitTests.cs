namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.UnitTests.Generation;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
public sealed class GenerationContractsUnitTests
{
    [Fact]
    public void LogitsBinding_Throws_WhenCategoryMissing()
    {
        var exception = Assert.Throws<ArgumentException>(() => new LogitsBinding("", "temperature", 0.5));
        Assert.Equal("category", exception.ParamName);
    }

    [Fact]
    public void LogitsBinding_Throws_WhenKindMissing()
    {
        var exception = Assert.Throws<ArgumentException>(() => new LogitsBinding("warper", "", 0.5));
        Assert.Equal("kind", exception.ParamName);
    }

    [Fact]
    public void LogitsBinding_Throws_WhenValueNotFinite()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new LogitsBinding("warper", "temperature", double.NaN));
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void LogitsBinding_AssignsProperties()
    {
        var binding = new LogitsBinding("warper", "temperature", 0.7);
        Assert.Equal("warper", binding.Category);
        Assert.Equal("temperature", binding.Kind);
        Assert.Equal(0.7, binding.Value);
        Assert.True(binding.IsWarper);
        Assert.False(binding.IsProcessor);
    }

    [Fact]
    public void StoppingCriterion_AssignsProperties()
    {
        var criterion = new StoppingCriterion(StoppingCriterionKinds.MaxNewTokens, 256, new[] { "END" });
        Assert.True(criterion.IsMaxNewTokens);
        Assert.False(criterion.IsStopSequences);
        Assert.Equal(256, criterion.Value);
        Assert.Equal(new[] { "END" }, criterion.Sequences);
    }

    [Fact]
    public void StoppingCriterion_IdentifiesStopSequences()
    {
        var criterion = new StoppingCriterion(StoppingCriterionKinds.StopSequences, null, new[] { "STOP" });
        Assert.True(criterion.IsStopSequences);
        Assert.False(criterion.IsMaxNewTokens);
    }

    [Fact]
    public void StreamingGenerationRequest_UsesResolvedSettings()
    {
        const string json = """
        {
            "temperature": 0.5,
            "skip_special_tokens": true
        }
        """;

        var settings = GenerationConfig.FromJson(json).BuildSettings();
        var request = new StreamingGenerationRequest("prompt", settings, messages: Array.Empty<ChatMessage>(), skipSpecialTokens: false);

        Assert.Same(settings, request.Settings);
        Assert.Equal("prompt", request.Prompt);
        Assert.False(request.SkipSpecialTokens);
        Assert.Same(settings.LogitsBindings, request.LogitsBindings);
        Assert.Single(request.LogitsBindings);
        Assert.Same(settings.StoppingCriteria, request.StoppingCriteria);
        Assert.Empty(request.StoppingCriteria);
    }

    [Fact]
    public void StreamingGenerationRequest_Throws_WhenPromptMissing()
    {
        const string json = "{\"temperature\": 0.5}";
        var settings = GenerationConfig.FromJson(json).BuildSettings();
        var exception = Assert.Throws<ArgumentException>(() => new StreamingGenerationRequest(" ", settings, null, true));
        Assert.Equal("prompt", exception.ParamName);
    }

    [Fact]
    public void GenerationOptions_StopSequencesNull_RemovesExisting()
    {
        const string json = """
        {
            "stop_sequences": ["</s>"]
        }
        """;

        var options = new GenerationOptions
        {
            StopSequences = null
        };

        var settings = GenerationConfig.FromJson(json).BuildSettings(options);
        Assert.Null(settings.StopSequences);
        Assert.Empty(settings.StoppingCriteria);
    }
}
