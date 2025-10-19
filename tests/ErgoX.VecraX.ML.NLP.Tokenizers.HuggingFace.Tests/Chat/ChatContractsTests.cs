namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Chat;

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
public sealed class ChatContractsTests
{
    [Fact]
    public void ChatMessage_from_text_assigns_defaults()
    {
        var metadata = new Dictionary<string, JsonNode?>
        {
            ["tenant"] = JsonValue.Create("vecrax"),
            ["region"] = null
        };

        var message = ChatMessage.FromText("user", "hello", metadata);
        Assert.Equal("user", message.Role);
        Assert.Equal("hello", message.Content);
        Assert.Empty(message.Parts);
    Assert.Equal("vecrax", message.Metadata["tenant"]?.GetValue<string>());

    metadata["tenant"] = JsonValue.Create("mutated");
    Assert.Equal("vecrax", message.Metadata["tenant"]?.GetValue<string>());
    Assert.True(message.Metadata.ContainsKey("region"));
    Assert.Null(message.Metadata["region"]);
    }

    [Fact]
    public void ChatMessage_from_parts_materializes_collection()
    {
        var parts = new List<ChatMessagePart>
        {
            new ChatTextPart("hi"),
            new ChatImageUrlPart("https://example/image.png", "image/png")
        };

        var message = ChatMessage.FromParts("assistant", parts);
        Assert.Equal("assistant", message.Role);
        Assert.Null(message.Content);
        Assert.Equal(2, message.Parts.Count);
        Assert.Contains(message.Parts, part => part is ChatTextPart text && text.Text == "hi");
    }

    [Fact]
    public void ChatMessage_validates_inputs()
    {
        Assert.Throws<ArgumentException>(() => new ChatMessage("", null, null, null));
        Assert.Throws<ArgumentException>(() => new ChatMessage("user", "text", Array.Empty<ChatMessagePart>(), null));

        var parts = new[] { new ChatTextPart("a") };
        Assert.Throws<ArgumentException>(() => new ChatMessage("user", "text", parts, null));
    }

    [Fact]
    public void ChatMessagePart_validates_payloads()
    {
        Assert.Throws<ArgumentException>(() => new ChatImageUrlPart(""));

        var payload = new JsonObject
        {
            ["type"] = "generic",
            ["value"] = 1
        };

        var generic = new ChatGenericPart(payload);
        var clone = generic.Payload;
        clone["value"] = JsonValue.Create(2);
        Assert.Equal(1, generic.Payload["value"]?.GetValue<int>());

        var serialized = generic.ToJson();
        Assert.Equal("generic", serialized["type"]?.GetValue<string>());

        var objectWithoutType = new JsonObject();
        Assert.Throws<ArgumentException>(() => new ChatGenericPart(objectWithoutType));
    }

    [Fact]
    public void ChatTemplateOptions_manages_variables()
    {
        var options = new ChatTemplateOptions
        {
            AddGenerationPrompt = false,
            TemplateOverride = "{{ custom }}"
        };

        options.SetVariable("custom", JsonValue.Create("value"));
        Assert.True(options.AdditionalVariables.ContainsKey("custom"));

        var clone = options.AdditionalVariables["custom"]?.DeepClone();
        Assert.Equal("value", clone?.GetValue<string>());

        Assert.True(options.RemoveVariable("custom"));
        Assert.False(options.RemoveVariable("custom"));
        Assert.False(options.RemoveVariable(""));
    }
}
