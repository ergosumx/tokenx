namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Generation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using Xunit;

public sealed class GenerationConfigTests
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
