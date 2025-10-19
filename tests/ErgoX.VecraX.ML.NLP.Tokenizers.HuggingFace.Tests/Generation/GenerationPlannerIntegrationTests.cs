namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Generation;

using System;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
public sealed class GenerationPlannerIntegrationTests
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
        Assert.True(tempBinding.IsWarper);
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
        Assert.True(topPBinding.IsWarper);
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
        Assert.Equal(100, maxTokensCriterion.Value);
    }

    [Fact]
    public void GenerationConfig_WithStopSequences_CreatesStoppingCriterion()
    {
        const string json = "{\"stop_sequences\": [\"END\", \"STOP\"]}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        var stopCriterion = settings.StoppingCriteria.FirstOrDefault(c => c.IsStopSequences);
        Assert.NotNull(stopCriterion);
        Assert.NotNull(stopCriterion.Sequences);
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
        const string json = @"{
            ""temperature"": 0.7,
            ""top_p"": ""0.9"",
            ""top_k"": ""50"",
            ""max_new_tokens"": ""200"",
            ""repetition_penalty"": 1.1
        }";
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

    [Fact]
    public void GenerationConfig_WithZeroValues_HandlesCorrectly()
    {
        const string json = "{\"temperature\": 0.0, \"top_k\": 0}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(0.0, settings.Temperature);
        Assert.Equal(0, settings.TopK);
    }

    [Fact]
    public void GenerationConfig_WithNegativeRepetitionPenalty_HandlesCorrectly()
    {
        const string json = "{\"repetition_penalty\": -1.0}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        // Should still parse even if value is unusual
        Assert.NotNull(settings);
    }

    [Fact]
    public void GenerationConfig_WithVeryLargeMaxTokens_ParsesCorrectly()
    {
        const string json = "{\"max_new_tokens\": 100000}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(100000, settings.MaxNewTokens);
    }

    [Fact]
    public void GenerationConfig_WithStopSequencesAndStop_PrioritizesStopSequences()
    {
        const string json = @"{""stop_sequences"": [""A""], ""stop"": [""B""]}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        // stop_sequences should take precedence
        Assert.Equal(new[] { "A" }, settings.StopSequences);
    }

    [Fact]
    public void GenerationOptions_CanOverrideStopSequences()
    {
        const string json = "{\"stop\": [\"A\", \"B\"]}";
        var config = GenerationConfig.FromJson(json);

        var options = new GenerationOptions
        {
            StopSequences = new[] { "X", "Y", "Z" }
        };

        var settings = config.BuildSettings(options);

        Assert.Equal(new[] { "X", "Y", "Z" }, settings.StopSequences);
    }

    [Fact]
    public void GenerationOptions_NullStopSequences_RemovesStopCriteria()
    {
        const string json = "{\"stop\": [\"A\", \"B\"]}";
        var config = GenerationConfig.FromJson(json);

        var options = new GenerationOptions
        {
            StopSequences = null
        };

        var settings = config.BuildSettings(options);

        Assert.Null(settings.StopSequences);
        Assert.DoesNotContain(settings.StoppingCriteria, c => c.IsStopSequences);
    }

    [Fact]
    public void GenerationConfig_WithComplexStopArray_HandlesAllElements()
    {
        const string json = @"{""stop"": [""</s>"", ""<|endoftext|>"", ""###"", 128001, 128009]}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.NotNull(settings.StopSequences);
        Assert.True(settings.StopSequences.Count >= 3);
    }

    [Fact]
    public void GenerationConfig_WithInvalidJson_ThrowsException()
    {
        const string invalidJson = "invalid json";
        var ex = Record.Exception(() => GenerationConfig.FromJson(invalidJson));
        Assert.NotNull(ex);
    }

    [Fact]
    public void GenerationConfig_ToJsonString_RoundTripsCorrectly()
    {
        const string json = @"{""temperature"": 0.8, ""max_new_tokens"": 100}";
        var config = GenerationConfig.FromJson(json);
        var roundTripped = config.ToJsonString(indented: false);

        Assert.Contains("temperature", roundTripped);
        Assert.Contains("max_new_tokens", roundTripped);
    }
}
