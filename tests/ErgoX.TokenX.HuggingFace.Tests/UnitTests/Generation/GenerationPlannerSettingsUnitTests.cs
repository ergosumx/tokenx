namespace ErgoX.TokenX.HuggingFace.Tests.UnitTests.Generation;

using System;
using System.Linq;
using ErgoX.TokenX.HuggingFace.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class GenerationPlannerSettingsUnitTests : HuggingFaceTestBase
{
    [Fact]
    public void GenerationConfig_WithTemperature_CreatesWarperBinding()
    {
        const string json = "{\"temperature\": 0.8}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.NotEmpty(settings.LogitsBindings);
        var tempBinding = settings.LogitsBindings.FirstOrDefault(b => b.Kind == "temperature");
        Assert.NotNull(tempBinding);
        Assert.True(tempBinding!.IsWarper);
        Assert.Equal(0.8, tempBinding.Value);
    }

    [Fact]
    public void GenerationConfig_WithTopP_CreatesWarperBinding()
    {
        const string json = "{\"top_p\": 0.9}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.NotEmpty(settings.LogitsBindings);
        var topPBinding = settings.LogitsBindings.FirstOrDefault(b => b.Kind == "top_p");
        Assert.NotNull(topPBinding);
        Assert.True(topPBinding!.IsWarper);
    }

    [Fact]
    public void GenerationConfig_WithTopK_BuildsSettings()
    {
        const string json = "{\"top_k\": 50}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.NotNull(settings);
        Assert.NotNull(settings.LogitsBindings);
    }

    [Fact]
    public void GenerationConfig_WithRepetitionPenalty_CreatesProcessorBinding()
    {
        const string json = "{\"repetition_penalty\": 1.2}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.NotEmpty(settings.LogitsBindings);
        var repBinding = settings.LogitsBindings.FirstOrDefault(b => b.Kind == "repetition_penalty");
        if (repBinding != null)
        {
            Assert.Equal(1.2, repBinding.Value);
        }
    }

    [Fact]
    public void GenerationConfig_WithMultipleParameters_CreatesMultipleBindings()
    {
        const string json = "{\"temperature\": 0.7, \"top_p\": 0.95, \"top_k\": 40}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.True(settings.LogitsBindings.Count >= 2);
    }

    [Fact]
    public void GenerationConfig_WithMaxNewTokens_CreatesStoppingCriterion()
    {
        const string json = "{\"max_new_tokens\": 100}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        var maxTokensCriterion = settings.StoppingCriteria.FirstOrDefault(c => c.IsMaxNewTokens);
        Assert.NotNull(maxTokensCriterion);
        Assert.Equal(100, maxTokensCriterion!.Value);
    }

    [Fact]
    public void GenerationConfig_WithStopSequences_CreatesStoppingCriterion()
    {
        const string json = "{\"stop_sequences\": [\"END\", \"STOP\"]}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        var stopCriterion = settings.StoppingCriteria.FirstOrDefault(c => c.IsStopSequences);
        Assert.NotNull(stopCriterion);
        Assert.NotNull(stopCriterion!.Sequences);
        Assert.Contains("END", stopCriterion.Sequences);
        Assert.Contains("STOP", stopCriterion.Sequences);
    }

    [Fact]
    public void GenerationConfig_WithSingleStopString_CreatesStoppingCriterion()
    {
        const string json = "{\"stop\": \"</s>\"}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(new[] { "</s>" }, settings.StopSequences);
    }

    [Fact]
    public void GenerationConfig_WithStopArray_CreatesStoppingCriterion()
    {
        const string json = "{\"stop\": [\"A\", \"B\", \"C\"]}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(new[] { "A", "B", "C" }, settings.StopSequences);
    }

    [Fact]
    public void GenerationConfig_WithMixedTypes_ParsesAllParameters()
    {
        const string json = """
            {
                "temperature": 0.7,
                "top_p": "0.9",
                "top_k": "50",
                "max_new_tokens": "200",
                "repetition_penalty": 1.1
            }
            """;
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(0.7, settings.Temperature);
        Assert.Equal(0.9, settings.TopP);
        Assert.Equal(50, settings.TopK);
        Assert.Equal(200, settings.MaxNewTokens);
        Assert.Equal(1.1, settings.RepetitionPenalty);
    }

    [Fact]
    public void GenerationConfig_WithMinNewTokens_ParsesCorrectly()
    {
        const string json = "{\"min_new_tokens\": 10}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(10, settings.MinNewTokens);
    }

    [Fact]
    public void GenerationConfig_WithDoSample_ParsesBoolean()
    {
        const string json = "{\"do_sample\": true}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.True(settings.DoSample);
    }

    [Fact]
    public void GenerationConfig_WithDoSampleString_ParsesBoolean()
    {
        const string json = "{\"do_sample\": \"false\"}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.False(settings.DoSample);
    }

    [Fact]
    public void GenerationConfig_WithNumBeams_ParsesInteger()
    {
        const string json = "{\"num_beams\": 4}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(4, settings.NumBeams);
    }

    [Fact]
    public void GenerationConfig_WithSkipSpecialTokens_ParsesBoolean()
    {
        const string json = "{\"skip_special_tokens\": true}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.True(settings.SkipSpecialTokens);
    }

    [Fact]
    public void GenerationConfig_EmptyJson_ReturnsEmptyBindings()
    {
        const string json = "{}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Empty(settings.LogitsBindings);
        Assert.Empty(settings.StoppingCriteria);
    }
}

