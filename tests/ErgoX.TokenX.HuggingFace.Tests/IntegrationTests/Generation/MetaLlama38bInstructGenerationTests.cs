namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Chat;
using ErgoX.TokenX.HuggingFace.Generation;
using ErgoX.TokenX.HuggingFace.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class MetaLlama38bInstructGenerationTests : HuggingFaceTestBase
{
    private const string ModelFolder = "meta-llama-3-8b-instruct";

    [Fact]
    public void LoadGenerationConfig_WhenPresent()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));
        Assert.True(tokenizer.SupportsGenerationDefaults);
        var defaults = tokenizer.GenerationConfig!.BuildSettings();

        Assert.Equal(512, defaults.MaxNewTokens);
        Assert.Equal(0.7, defaults.Temperature);
        Assert.Equal(0.9, defaults.TopP);
        Assert.Equal(1.1, defaults.RepetitionPenalty);
        Assert.Equal(new[] { "<|eot_id|>", "</s>" }, defaults.StopSequences);
        Assert.True(defaults.SkipSpecialTokens);

        // Planner functionality removed
        Assert.Empty(defaults.LogitsBindings);
        Assert.Empty(defaults.StoppingCriteria);
    }

    [Fact]
    public void Generate_AppliesOverrides()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));

        var overrides = new GenerationOptions
        {
            Temperature = 0.2,
            TopP = 0.8,
            StopSequences = new[] { "###" }
        };
        overrides.AdditionalParameters["custom_parameter"] = JsonValue.Create(42);

        var request = tokenizer.Generate("Hello world", overrides);

        Assert.Equal("Hello world", request.Prompt);
        Assert.Null(request.Messages);

        var settings = request.Settings;
        Assert.Equal(0.2, settings.Temperature);
        Assert.Equal(0.8, settings.TopP);
        Assert.Equal(512, settings.MaxNewTokens);
        Assert.Equal(new[] { "###" }, settings.StopSequences);
        Assert.True(settings.TryGetRawParameter("custom_parameter", out var customNode));
        Assert.Equal(42, customNode!.GetValue<int>());

        // Planner functionality removed
        Assert.Empty(settings.LogitsBindings);
        Assert.Empty(settings.StoppingCriteria);
    }

    [Fact]
    public void GenerateFromMessages_UsesChatTemplateDefaults()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));
        Assert.True(tokenizer.SupportsChatTemplate);

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromText("system", "You are a helpful model."),
            ChatMessage.FromText("user", "Hello!")
        };

        var request = tokenizer.Generate(messages, generationOptions: new GenerationOptions { DoSample = false });

        Assert.NotNull(request.Messages);
        Assert.Equal(2, request.Messages!.Count);
        Assert.False(string.IsNullOrWhiteSpace(request.Prompt));
        Assert.False(request.Settings.DoSample);
        Assert.Equal(0.7, request.Settings.Temperature);

        // Planner functionality removed
        Assert.Empty(request.LogitsBindings);
        Assert.Empty(request.StoppingCriteria);
    }

    [Fact]
    public void Generate_RemovesBindingsWhenParametersNeutralized()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));

        var options = new GenerationOptions
        {
            Temperature = null,
            TopP = 1.0,
            RepetitionPenalty = 1.0
        };

        var request = tokenizer.Generate("Hello world", options);
        Assert.Empty(request.LogitsBindings);
        Assert.Equal(1.0, request.Settings.TopP);
        Assert.Null(request.Settings.Temperature);
        Assert.Equal(1.0, request.Settings.RepetitionPenalty);
    }

    [Fact]
    public void Generate_RemovesStoppingCriteriaWhenCleared()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));

        var options = new GenerationOptions
        {
            MaxNewTokens = null,
            StopSequences = null
        };

        var request = tokenizer.Generate("Hello world", options);
        Assert.Empty(request.StoppingCriteria);
        Assert.Null(request.Settings.MaxNewTokens);
        Assert.Null(request.Settings.StopSequences);
    }

    [Fact]
    public void GenerateStream_InfersSkipSpecialTokens()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));
        var streamRequest = tokenizer.GenerateStream("Hello world");

        Assert.Equal("Hello world", streamRequest.Prompt);
        Assert.True(streamRequest.SkipSpecialTokens);
        Assert.Equal(tokenizer.Generate("Hello world").StoppingCriteria.Count, streamRequest.StoppingCriteria.Count);

        var overrideRequest = tokenizer.GenerateStream(
            "Hello world",
            streamOptions: new StreamGenerationOptions { SkipSpecialTokens = false });

        Assert.False(overrideRequest.SkipSpecialTokens);
    }

    [Fact]
    public void GenerateStream_FromMessagesIncludesContext()
    {
        using var tokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ModelFolder));

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromText("system", "You are a helpful model."),
            ChatMessage.FromText("user", "Stream this response")
        };

        var request = tokenizer.GenerateStream(messages);

        Assert.NotNull(request.Messages);
        Assert.Equal(2, request.Messages!.Count);
        Assert.True(request.SkipSpecialTokens);
        // Planner functionality removed
        Assert.Empty(request.StoppingCriteria);
        Assert.Empty(request.LogitsBindings);
    }

    [Fact]
    public void SystemPrimingWithFollowUpRequest()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "System priming with follow-up request");
    }

    [Fact]
    public void AssistantFinalizesRemediationGuidance()
    {
        GenerationTestUtilities.AssertChatTemplateCase(ModelFolder, "Assistant finalizes remediation guidance");
    }
}

