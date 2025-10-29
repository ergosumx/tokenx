namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.UnitTests.Options;

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class OptionsContractsUnitTests : HuggingFaceTestBase
{
    [Fact]
    public void AddedToken_validates_and_clones()
    {
        var token = new AddedToken("<|start|>", isSpecial: true, singleWord: true, leftStrip: true, rightStrip: false, normalized: false);
        Assert.Equal("<|start|>", token.Content);
        Assert.True(token.IsSpecial);
        Assert.False(token.WithSpecial(false).IsSpecial);

        var defaultToken = new AddedToken("regular");
        Assert.True(defaultToken.Normalized);

        var specialDefault = new AddedToken("<special>", isSpecial: true);
        Assert.False(specialDefault.Normalized);

        Assert.Throws<ArgumentException>(() => new AddedToken(""));
    }

    [Fact]
    public void PaddingOptions_enforces_constraints()
    {
        var options = new PaddingOptions(PaddingDirection.Left, padId: 10, padTypeId: 20, padToken: "<pad>", length: 128, padToMultipleOf: 1);
        Assert.Equal(PaddingDirection.Left, options.Direction);
        Assert.Equal((uint)10, options.PadId);
        Assert.Null(options.PadToMultipleOf);

        Assert.Throws<ArgumentOutOfRangeException>(() => new PaddingOptions(padId: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaddingOptions(padId: 0, padTypeId: -2));
        Assert.Throws<ArgumentException>(() => new PaddingOptions(padToken: ""));
    }

    [Fact]
    public void TruncationOptions_enforces_constraints()
    {
        var options = new TruncationOptions(maxLength: 64, stride: 4, TruncationStrategy.OnlySecond, TruncationDirection.Left);
        Assert.Equal(64, options.MaxLength);
        Assert.Equal(4, options.Stride);
        Assert.Equal(TruncationStrategy.OnlySecond, options.Strategy);
        Assert.Equal(TruncationDirection.Left, options.Direction);

        Assert.Throws<ArgumentOutOfRangeException>(() => new TruncationOptions(maxLength: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TruncationOptions(maxLength: 1, stride: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TruncationOptions(maxLength: 1, strategy: (TruncationStrategy)123));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TruncationOptions(maxLength: 1, direction: (TruncationDirection)123));
    }

    [Fact]
    public void GenerationOptions_track_specified_state_via_settings()
    {
        const string json = "{\"temperature\": 0.9, \"stop\": \"END\"}";
        var config = GenerationConfig.FromJson(json);

        var overrides = new GenerationOptions
        {
            Temperature = 0.4,
            TopP = 0.5,
            TopK = 10,
            RepetitionPenalty = 1.5,
            MaxNewTokens = 32,
            MinNewTokens = 8,
            DoSample = false,
            NumBeams = 3,
            StopSequences = new[] { "STOP" }
        };

        overrides.AdditionalParameters["custom"] = JsonValue.Create("value");
        overrides.AdditionalParameters["stop"] = null;

        var settings = config.BuildSettings(overrides);
        Assert.Equal(0.4, settings.Temperature);
        Assert.Equal(0.5, settings.TopP);
        Assert.Equal(10, settings.TopK);
        Assert.Equal(1.5, settings.RepetitionPenalty);
        Assert.Equal(32, settings.MaxNewTokens);
        Assert.Equal(8, settings.MinNewTokens);
        Assert.False(settings.DoSample);
        Assert.Equal(3, settings.NumBeams);
        Assert.Equal(new[] { "STOP" }, settings.StopSequences);
        Assert.True(settings.TryGetRawParameter("custom", out var node));
        Assert.Equal("value", node?.GetValue<string>());
        Assert.False(settings.TryGetRawParameter("stop", out _));
    }

    [Fact]
    public void GenerationSettings_supports_case_insensitive_lookup_and_string_lists()
    {
        const string json = "{\"STOP\": [\"A\", \"B\"], \"max_new_tokens\": \"10\"}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(new[] { "A", "B" }, settings.StopSequences);
        Assert.Equal(10, settings.MaxNewTokens);
        Assert.True(settings.TryGetRawParameter("STOP", out var raw));
        Assert.Equal(JsonValueKind.Array, raw!.GetValueKind());
    }

    [Fact]
    public void GenerationSettings_parses_mixed_value_types()
    {
        const string json = "{\"temperature\":\"0.5\",\"top_p\":0.83,\"top_k\":\"12\",\"repetition_penalty\":\"1.05\",\"max_new_tokens\":\"16\",\"min_new_tokens\":2,\"do_sample\":\"true\",\"num_beams\":\"4\",\"stop_sequences\":[\"###\",42],\"skip_special_tokens\":\"false\",\"custom_flag\":\"true\",\"custom_numeric\":1}";
        var config = GenerationConfig.FromJson(json);
        var settings = config.BuildSettings();

        Assert.Equal(0.5, settings.Temperature);
        Assert.Equal(0.83, settings.TopP);
        Assert.Equal(12, settings.TopK);
        Assert.Equal(1.05, settings.RepetitionPenalty);
        Assert.Equal(16, settings.MaxNewTokens);
        Assert.Equal(2, settings.MinNewTokens);
        Assert.True(settings.DoSample);
        Assert.Equal(4, settings.NumBeams);
        Assert.Equal(new[] { "###", "42" }, settings.StopSequences);
        Assert.False(settings.SkipSpecialTokens);

        Assert.True(settings.TryGetRawParameter("custom_flag", out var boolNode));
        Assert.Equal("true", boolNode!.GetValue<string>());
        Assert.True(settings.TryGetRawParameter("CUSTOM_NUMERIC", out var numericNode));
        Assert.Equal(JsonValueKind.Number, numericNode!.GetValueKind());
        Assert.False(settings.TryGetRawParameter("   ", out _));
    }

    [Fact]
    public void TokenizerConfig_parses_added_tokens_into_vocab()
    {
        const string json = "{\"model\":{\"vocab\":{},\"unk_token\":\"<eos>\"},\"added_tokens\":[{\"id\":10,\"content\":\"<eos>\"}],\"model_max_length\":512}";
        var config = TokenizerConfig.FromJson(json);
        Assert.Equal(512, config.ModelMaxLength);
        Assert.Equal(10, config.Vocab["<eos>"]);
        Assert.Equal(10, config.UnknownTokenId);

        var roundTrip = config.ToJson(pretty: false);
        Assert.Contains("\"model_max_length\":512", roundTrip, StringComparison.Ordinal);
    }

    [Fact]
    public void SpecialTokensMap_deserializes_payloads()
    {
        const string json = "{\"bos_token\":{\"id\":1,\"content\":\"<s>\"},\"additional_special_tokens\":[{\"id\":2,\"content\":\"</s>\"}]}";
        var map = SpecialTokensMap.FromJson(json);
        Assert.NotNull(map);
        Assert.Equal("<s>", map!.BosToken?.Content);
        Assert.Single(map.AdditionalSpecialTokens);
    }
}
