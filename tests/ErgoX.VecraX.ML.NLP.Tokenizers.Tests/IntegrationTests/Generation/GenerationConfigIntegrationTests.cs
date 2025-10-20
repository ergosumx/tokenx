namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Generation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class GenerationConfigIntegrationTests
{
    private const string SolutionFileName = "TokenX.HF.sln";

    [Fact]
    public void LoadGenerationConfig_WhenPresent()
    {
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));
        Assert.True(tokenizer.SupportsGenerationDefaults);
        var defaults = tokenizer.GenerationConfig!.BuildSettings();

        Assert.Equal(512, defaults.MaxNewTokens);
        Assert.Equal(0.7, defaults.Temperature);
        Assert.Equal(0.9, defaults.TopP);
        Assert.Equal(1.1, defaults.RepetitionPenalty);
        Assert.Equal(new[] { "<|eot_id|>", "</s>" }, defaults.StopSequences);
        Assert.True(defaults.SkipSpecialTokens);

        var bindings = defaults.LogitsBindings;
        Assert.Equal(3, bindings.Count);
        Assert.Contains(bindings, binding => binding.IsWarper && binding.Kind == "temperature" && Math.Abs(binding.Value - 0.7) < 1e-9);
        Assert.Contains(bindings, binding => binding.IsWarper && binding.Kind == "top_p" && Math.Abs(binding.Value - 0.9) < 1e-9);
        Assert.Contains(bindings, binding => binding.IsProcessor && binding.Kind == "repetition_penalty" && Math.Abs(binding.Value - 1.1) < 1e-9);

        var stopping = defaults.StoppingCriteria;
        Assert.Equal(2, stopping.Count);
        Assert.Contains(stopping, criterion => criterion.IsMaxNewTokens && criterion.Value == 512);
        Assert.Contains(stopping, criterion => criterion.IsStopSequences && criterion.Sequences!.SequenceEqual(new[] { "<|eot_id|>", "</s>" }));
    }

    [Fact]
    public void Generate_AppliesOverrides()
    {
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));

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

        var bindings = settings.LogitsBindings;
        Assert.Equal(3, bindings.Count);
        Assert.Contains(bindings, binding => binding.Kind == "temperature" && Math.Abs(binding.Value - 0.2) < 1e-9);
        Assert.Contains(bindings, binding => binding.Kind == "top_p" && Math.Abs(binding.Value - 0.8) < 1e-9);
        Assert.Contains(bindings, binding => binding.Kind == "repetition_penalty" && Math.Abs(binding.Value - 1.1) < 1e-9);

        var stopping = settings.StoppingCriteria;
        Assert.Equal(2, stopping.Count);
        Assert.Contains(stopping, criterion => criterion.IsMaxNewTokens && criterion.Value == 512);
        Assert.Contains(stopping, criterion => criterion.IsStopSequences && criterion.Sequences!.SequenceEqual(new[] { "###" }));
    }

    [Fact]
    public void GenerateFromMessages_UsesChatTemplateDefaults()
    {
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));
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
        Assert.Equal(false, request.Settings.DoSample);
        Assert.Equal(0.7, request.Settings.Temperature);

        var bindings = request.LogitsBindings;
        Assert.Equal(3, bindings.Count);
        Assert.Contains(bindings, binding => binding.Kind == "temperature" && Math.Abs(binding.Value - 0.7) < 1e-9);
        Assert.Contains(bindings, binding => binding.Kind == "top_p" && Math.Abs(binding.Value - 0.9) < 1e-9);
        Assert.Contains(bindings, binding => binding.Kind == "repetition_penalty" && Math.Abs(binding.Value - 1.1) < 1e-9);

        var stopping = request.StoppingCriteria;
        Assert.Equal(2, stopping.Count);
        Assert.Contains(stopping, criterion => criterion.IsMaxNewTokens && criterion.Value == 512);
        Assert.Contains(stopping, criterion => criterion.IsStopSequences && criterion.Sequences!.SequenceEqual(new[] { "<|eot_id|>", "</s>" }));
    }

    [Fact]
    public void Generate_RemovesBindingsWhenParametersNeutralized()
    {
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));

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
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));

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
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));
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
        using var tokenizer = AutoTokenizer.Load(GetModelRoot("meta-llama-3-8b-instruct"));

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromText("system", "You are a helpful model."),
            ChatMessage.FromText("user", "Stream this response")
        };

        var request = tokenizer.GenerateStream(messages);

        Assert.NotNull(request.Messages);
        Assert.Equal(2, request.Messages!.Count);
        Assert.True(request.SkipSpecialTokens);
        Assert.NotEmpty(request.StoppingCriteria);
        Assert.NotEmpty(request.LogitsBindings);
    }

    private static string GetModelRoot(string model)
    {
        var root = GetBenchmarksDataRoot();
        return Path.Combine(root, model);
    }

    private static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_TestData");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}
