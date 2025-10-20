namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.AutoTokenizer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class AutoTokenizerIntegrationTests
{
    private const string ChatModel = "meta-llama-3-8b-instruct";
    private const string SimpleModel = "gpt2";

    [Fact]
    public void Load_applies_defaults_and_exposes_metadata()
    {
        using var autoTokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ChatModel));
        Assert.NotNull(autoTokenizer.TokenizerConfig);
        Assert.NotNull(autoTokenizer.GenerationConfig);
        Assert.True(autoTokenizer.SupportsChatTemplate);
        Assert.True(autoTokenizer.SupportsGenerationDefaults);

        if (autoTokenizer.SpecialTokens is not null)
        {
            Assert.NotNull(autoTokenizer.SpecialTokens.BosToken);
        }

        var padding = autoTokenizer.Tokenizer.GetPadding();
        var truncation = autoTokenizer.Tokenizer.GetTruncation();
        Assert.True(padding is null || padding.Length.HasValue);
        Assert.True(truncation is null || truncation.MaxLength > 0);
    }

    [Fact]
    public async Task Load_async_without_generation_defaults_disables_generation()
    {
        var options = new AutoTokenizerLoadOptions
        {
            LoadGenerationConfig = false,
            ApplyTokenizerDefaults = false
        };

        var autoTokenizer = await AutoTokenizer.LoadAsync(TestDataPath.GetModelRoot(SimpleModel), options).ConfigureAwait(false);
        try
        {
            Assert.False(autoTokenizer.SupportsGenerationDefaults);
            Assert.Null(autoTokenizer.GenerationConfig);
            Assert.Null(autoTokenizer.Tokenizer.GetPadding());
            Assert.Null(autoTokenizer.Tokenizer.GetTruncation());

            Assert.Throws<InvalidOperationException>(() => autoTokenizer.Generate("prompt"));
        }
        finally
        {
            autoTokenizer.Dispose();
        }
    }

    [Fact]
    public void Generate_from_prompt_and_messages_respects_options()
    {
        using var autoTokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ChatModel));

        var numericOverrides = new GenerationOptions
        {
            TopP = null,
            DoSample = false
        };

        numericOverrides.AdditionalParameters["skip_special_tokens"] = JsonValue.Create(0.0);

        var promptRequest = autoTokenizer.Generate("Hello world", numericOverrides);

        Assert.Equal("Hello world", promptRequest.Prompt);
        Assert.Null(promptRequest.Messages);
        Assert.Equal(0.7, promptRequest.Settings.Temperature);
        Assert.Null(promptRequest.Settings.TopP);
        Assert.False(promptRequest.Settings.DoSample);

        var defaultStream = autoTokenizer.GenerateStream("Hello world");
        Assert.True(defaultStream.SkipSpecialTokens);

        var streamWithOverride = autoTokenizer.GenerateStream("Hello world", numericOverrides);
        Assert.False(streamWithOverride.SkipSpecialTokens);

        var stringOverrides = new GenerationOptions();
        stringOverrides.AdditionalParameters["skip_special_tokens"] = JsonValue.Create("true");
        var streamFromString = autoTokenizer.GenerateStream("Hello world", stringOverrides);
        Assert.True(streamFromString.SkipSpecialTokens);

        var messages = new List<ChatMessage>
        {
            ChatMessage.FromText("system", "You are friendly."),
            ChatMessage.FromText("user", "Hi there")
        };

        var chatOptionsWithVariable = new ChatTemplateOptions();
        chatOptionsWithVariable.SetVariable("custom", JsonValue.Create("value"));

        var promptWithVariables = autoTokenizer.ApplyChatTemplate(messages, chatOptionsWithVariable);
        Assert.False(string.IsNullOrWhiteSpace(promptWithVariables));

        var ids = autoTokenizer.ApplyChatTemplateAsTokenIds(messages, chatOptionsWithVariable);
        Assert.NotEmpty(ids);

        var generationOptions = new GenerationOptions
        {
            Temperature = 0.3,
            TopP = 0.95,
            StopSequences = new[] { "###" }
        };

        var chatRequest = autoTokenizer.Generate(messages, chatOptionsWithVariable, generationOptions);
        Assert.Equal(messages.Count, chatRequest.Messages!.Count);
        Assert.False(string.IsNullOrWhiteSpace(chatRequest.Prompt));

        var streamRequest = autoTokenizer.GenerateStream(messages, chatOptionsWithVariable);
        Assert.NotEmpty(streamRequest.Prompt);

        streamRequest = autoTokenizer.GenerateStream("Hello world", generationOptions, new StreamGenerationOptions { SkipSpecialTokens = false });
        Assert.False(streamRequest.SkipSpecialTokens);

        streamRequest = autoTokenizer.GenerateStream("Hello world", generationOptions, new StreamGenerationOptions { SkipSpecialTokens = true });
        Assert.True(streamRequest.SkipSpecialTokens);
    }

    [Fact]
    public void Load_applies_padding_and_truncation_variants()
    {
        var config = new TokenizerConfig
        {
            Padding = new TokenizerConfig.SerializedPadding
            {
                Direction = "left",
                PadId = 101,
                PadTypeId = 3,
                PadToken = string.Empty,
                Length = 8,
                PadToMultipleOf = 1
            },
            Truncation = new TokenizerConfig.SerializedTruncation
            {
                MaxLength = 12,
                Stride = 2,
                Strategy = "only_second",
                Direction = "left"
            }
        };

        var path = CreateTemporaryModel(SimpleModel, config);
        try
        {
            using var autoTokenizer = AutoTokenizer.Load(path);

            var padding = autoTokenizer.Tokenizer.GetPadding();
            Assert.NotNull(padding);
            Assert.Equal(PaddingDirection.Left, padding!.Direction);
            Assert.Equal((uint)101, padding.PadId);
            Assert.Equal("[PAD]", padding.PadToken);
            Assert.Equal(8, padding.Length);
            Assert.Null(padding.PadToMultipleOf);

            var truncation = autoTokenizer.Tokenizer.GetTruncation();
            Assert.NotNull(truncation);
            Assert.Equal(12, truncation!.MaxLength);
            Assert.Equal(2, truncation.Stride);
            Assert.Equal(TruncationStrategy.OnlySecond, truncation.Strategy);
            Assert.Equal(TruncationDirection.Left, truncation.Direction);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Theory]
    [InlineData("only_first", "right", "<pad>", PaddingDirection.Right, TruncationStrategy.OnlyFirst, TruncationDirection.Right)]
    [InlineData("only_second", "left", "custom_pad", PaddingDirection.Left, TruncationStrategy.OnlySecond, TruncationDirection.Left)]
    [InlineData("unknown", null, "provided", PaddingDirection.Right, TruncationStrategy.LongestFirst, TruncationDirection.Right)]
    public void Load_maps_truncation_and_padding_settings(
        string strategy,
        string? truncationDirection,
        string padToken,
        PaddingDirection expectedPadding,
        TruncationStrategy expectedStrategy,
        TruncationDirection expectedDirection)
    {
        var config = new TokenizerConfig
        {
            Padding = new TokenizerConfig.SerializedPadding
            {
                Direction = expectedPadding == PaddingDirection.Left ? "left" : "right",
                PadId = 7,
                PadTypeId = 2,
                PadToken = padToken,
                Length = 4,
                PadToMultipleOf = 4
            },
            Truncation = new TokenizerConfig.SerializedTruncation
            {
                MaxLength = 9,
                Stride = 1,
                Strategy = strategy,
                Direction = truncationDirection ?? string.Empty
            }
        };

        var path = CreateTemporaryModel(SimpleModel, config);
        try
        {
            using var autoTokenizer = AutoTokenizer.Load(path);

            var padding = autoTokenizer.Tokenizer.GetPadding();
            Assert.NotNull(padding);
            Assert.Equal(expectedPadding, padding!.Direction);
            Assert.Equal(padToken, padding.PadToken);
            Assert.Equal(4, padding.PadToMultipleOf);

            var truncation = autoTokenizer.Tokenizer.GetTruncation();
            Assert.NotNull(truncation);
            Assert.Equal(expectedStrategy, truncation!.Strategy);
            Assert.Equal(expectedDirection, truncation.Direction);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void Load_throws_when_padding_configuration_is_invalid()
    {
        var config = new TokenizerConfig
        {
            Padding = new TokenizerConfig.SerializedPadding
            {
                Direction = "left",
                PadId = -1
            }
        };

        var path = CreateTemporaryModel(SimpleModel, config);
        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => AutoTokenizer.Load(path));
            Assert.Contains("padding configuration", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void Load_throws_when_truncation_configuration_is_invalid()
    {
        var config = new TokenizerConfig
        {
            Truncation = new TokenizerConfig.SerializedTruncation
            {
                MaxLength = -5
            }
        };

        var path = CreateTemporaryModel(SimpleModel, config);
        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => AutoTokenizer.Load(path));
            Assert.Contains("truncation configuration", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void Apply_chat_template_validates_inputs()
    {
        using var autoTokenizer = AutoTokenizer.Load(TestDataPath.GetModelRoot(ChatModel));

        Assert.Throws<ArgumentNullException>(() => autoTokenizer.Generate((string)null!));
        Assert.Throws<ArgumentException>(() => autoTokenizer.Generate(Array.Empty<ChatMessage>()));

        var invalidMessages = new ChatMessage[] { null! };
        Assert.Throws<ArgumentException>(() => autoTokenizer.Generate((IEnumerable<ChatMessage>)invalidMessages));
    }

    private static string CreateTemporaryModel(string sourceModel, TokenizerConfig config)
    {
        var root = Path.Combine(Path.GetTempPath(), "vecrax-hf-tokenizers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var sourceRoot = TestDataPath.GetModelRoot(sourceModel);
        File.Copy(Path.Combine(sourceRoot, "tokenizer.json"), Path.Combine(root, "tokenizer.json"), overwrite: true);

        var json = config.ToJson(pretty: true);
        File.WriteAllText(Path.Combine(root, "tokenizer_config.json"), json);

        return root;
    }
}
