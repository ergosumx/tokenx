namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.UnitTests.Generation;

using System;
using System.Linq;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class GenerationSettingsUnitTests : HuggingFaceTestBase
{
    [Fact]
    public void BuildSettings_WithSimpleJson_ComputesBindingsAndCriteria()
    {
        const string json = """
        {
            "temperature": 0.6,
            "top_p": 0.85,
            "repetition_penalty": 1.2,
            "max_new_tokens": 64,
            "stop_sequences": ["END", "STOP"],
            "skip_special_tokens": true
        }
        """;

        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(0.6, settings.Temperature);
        Assert.Equal(0.85, settings.TopP);
        Assert.Equal(1.2, settings.RepetitionPenalty);
        Assert.Equal(64, settings.MaxNewTokens);
        Assert.Equal(new[] { "END", "STOP" }, settings.StopSequences);
        Assert.True(settings.SkipSpecialTokens);

        Assert.Collection(
            settings.LogitsBindings,
            binding => Assert.True(binding.IsWarper && binding.Kind == "temperature"),
            binding => Assert.True(binding.IsWarper && binding.Kind == "top_p"),
            binding => Assert.True(binding.IsProcessor && binding.Kind == "repetition_penalty"));

        Assert.Collection(
            settings.StoppingCriteria,
            criterion => Assert.True(criterion.IsMaxNewTokens && criterion.Value == 64),
            criterion => Assert.True(criterion.IsStopSequences && criterion.Sequences!.SequenceEqual(new[] { "END", "STOP" })));
    }

    [Fact]
    public void BuildSettings_WithNeutralParameters_DropsDerivedBindings()
    {
        const string json = """
        {
            "temperature": 1.0,
            "top_p": 1.0,
            "repetition_penalty": 1.0,
            "max_new_tokens": 0
        }
        """;

        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Empty(settings.LogitsBindings);
        Assert.Empty(settings.StoppingCriteria);
    }

    [Fact]
    public void BuildSettings_WithOverrides_UpdatesSnapshot()
    {
        const string json = """
        {
            "temperature": 0.9,
            "top_p": 0.5,
            "max_new_tokens": 32,
            "stop_sequences": ["DONE"]
        }
        """;

        var options = new GenerationOptions
        {
            Temperature = 0.4,
            TopP = null,
            MaxNewTokens = 128,
            StopSequences = new[] { "CUSTOM" },
            DoSample = false
        };

        options.AdditionalParameters["custom_parameter"] = 17;

        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings(options);

        Assert.Equal(0.4, settings.Temperature);
        Assert.Null(settings.TopP);
        Assert.Equal(128, settings.MaxNewTokens);
    Assert.False(settings.DoSample);
        Assert.Equal(new[] { "CUSTOM" }, settings.StopSequences);
        Assert.True(settings.TryGetRawParameter("custom_parameter", out var raw));
        Assert.Equal(17, raw!.GetValue<int>());

        var jsonObject = settings.ToJsonObject();
        jsonObject["max_new_tokens"] = 10;
        Assert.Equal(128, settings.MaxNewTokens);
    }

    [Fact]
    public void TryGetRawParameter_ReturnsCloneWhenPresent()
    {
        const string json = """
        {
            "temperature": 0.6,
            "metadata": {
                "provider": "vecrax"
            }
        }
        """;

        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();
        Assert.True(settings.TryGetRawParameter("metadata", out var raw));
        Assert.NotNull(raw);

        raw!["provider"] = JsonValue.Create("mutated");
        Assert.Equal("vecrax", settings.ToJsonObject()["metadata"]?["provider"]?.GetValue<string>());
        Assert.Equal("mutated", raw["provider"]?.GetValue<string>());
    }
}
