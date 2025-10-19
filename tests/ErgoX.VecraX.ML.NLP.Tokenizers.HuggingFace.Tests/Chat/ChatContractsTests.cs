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
    public void ChatTextPart_serializes_correctly()
    {
        var textPart = new ChatTextPart("Hello world");
        Assert.Equal("text", textPart.Type);
        Assert.Equal("Hello world", textPart.Text);

        var json = textPart.ToJson();
        Assert.Equal("text", json["type"]?.GetValue<string>());
        Assert.Equal("Hello world", json["text"]?.GetValue<string>());
    }

    [Fact]
    public void ChatTextPart_handles_empty_text()
    {
        var textPart = new ChatTextPart(null!);
        Assert.Equal(string.Empty, textPart.Text);

        var json = textPart.ToJson();
        Assert.Equal(string.Empty, json["text"]?.GetValue<string>());
    }

    [Fact]
    public void ChatImageUrlPart_serializes_with_mime_type()
    {
        var imagePart = new ChatImageUrlPart("https://example.com/image.jpg", "image/jpeg");
        Assert.Equal("image_url", imagePart.Type);
        Assert.Equal("https://example.com/image.jpg", imagePart.Url);
        Assert.Equal("image/jpeg", imagePart.MimeType);

        var json = imagePart.ToJson();
        Assert.Equal("image_url", json["type"]?.GetValue<string>());
        var imageUrl = json["image_url"] as JsonObject;
        Assert.NotNull(imageUrl);
        Assert.Equal("https://example.com/image.jpg", imageUrl["url"]?.GetValue<string>());
        Assert.Equal("image/jpeg", imageUrl["mime_type"]?.GetValue<string>());
    }

    [Fact]
    public void ChatImageUrlPart_serializes_without_mime_type()
    {
        var imagePart = new ChatImageUrlPart("https://example.com/image.png");
        Assert.Null(imagePart.MimeType);

        var json = imagePart.ToJson();
        var imageUrl = json["image_url"] as JsonObject;
        Assert.NotNull(imageUrl);
        Assert.Equal("https://example.com/image.png", imageUrl["url"]?.GetValue<string>());
        Assert.False(imageUrl.ContainsKey("mime_type"));
    }

    [Fact]
    public void ChatImageUrlPart_validates_url()
    {
        Assert.Throws<ArgumentException>(() => new ChatImageUrlPart(null!));
        Assert.Throws<ArgumentException>(() => new ChatImageUrlPart(""));
        Assert.Throws<ArgumentException>(() => new ChatImageUrlPart("   "));
    }

    [Fact]
    public void ChatGenericPart_validates_null_payload()
    {
        Assert.Throws<ArgumentNullException>(() => new ChatGenericPart(null!));
    }

    [Fact]
    public void ChatGenericPart_validates_type_property()
    {
        var payloadWithoutType = new JsonObject();
        Assert.Throws<ArgumentException>(() => new ChatGenericPart(payloadWithoutType));

        var payloadWithNullType = new JsonObject { ["type"] = null };
        Assert.Throws<ArgumentException>(() => new ChatGenericPart(payloadWithNullType));

        var payloadWithEmptyType = new JsonObject { ["type"] = JsonValue.Create("") };
        Assert.Throws<ArgumentException>(() => new ChatGenericPart(payloadWithEmptyType));

        var payloadWithWhitespaceType = new JsonObject { ["type"] = JsonValue.Create("   ") };
        Assert.Throws<ArgumentException>(() => new ChatGenericPart(payloadWithWhitespaceType));
    }

    [Fact]
    public void ChatGenericPart_clones_payload_on_construction()
    {
        var payload = new JsonObject
        {
            ["type"] = "test",
            ["data"] = JsonValue.Create(42)
        };

        var genericPart = new ChatGenericPart(payload);

        payload["data"] = JsonValue.Create(99);

        Assert.Equal(42, genericPart.Payload["data"]?.GetValue<int>());
    }

    [Fact]
    public void ChatGenericPart_returns_cloned_payload()
    {
        var payload = new JsonObject
        {
            ["type"] = "test",
            ["data"] = JsonValue.Create(42)
        };

        var genericPart = new ChatGenericPart(payload);
        var retrieved = genericPart.Payload;

        retrieved["data"] = JsonValue.Create(99);

        Assert.Equal(42, genericPart.Payload["data"]?.GetValue<int>());
    }

    [Fact]
    public void ChatGenericPart_to_json_returns_clone()
    {
        var payload = new JsonObject
        {
            ["type"] = "test",
            ["data"] = JsonValue.Create(42)
        };

        var genericPart = new ChatGenericPart(payload);
        var json = genericPart.ToJson();

        json["data"] = JsonValue.Create(99);

        Assert.Equal(42, genericPart.ToJson()["data"]?.GetValue<int>());
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
