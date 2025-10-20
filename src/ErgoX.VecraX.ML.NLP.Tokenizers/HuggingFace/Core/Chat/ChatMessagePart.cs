namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Text.Json.Nodes;

public abstract class ChatMessagePart
{
    protected ChatMessagePart(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Part type must be provided.", nameof(type));
        }

        Type = type;
    }

    public string Type { get; }

    protected static JsonObject CreateBaseObject(string type)
    {
        var payload = new JsonObject
        {
            ["type"] = JsonValue.Create(type)
        };
        return payload;
    }

    public abstract JsonObject ToJson();
}

public sealed class ChatTextPart : ChatMessagePart
{
    public ChatTextPart(string text)
        : base("text")
    {
        Text = text ?? string.Empty;
    }

    public string Text { get; }

    public override JsonObject ToJson()
    {
        var payload = CreateBaseObject(Type);
        payload["text"] = JsonValue.Create(Text);
        return payload;
    }
}

public sealed class ChatImageUrlPart : ChatMessagePart
{
    public ChatImageUrlPart(string url, string? mimeType = null)
        : base("image_url")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Image URL must be provided.", nameof(url));
        }

        Url = url;
        MimeType = mimeType;
    }

    public string Url { get; }

    public string? MimeType { get; }

    public override JsonObject ToJson()
    {
        var payload = CreateBaseObject(Type);
        var image = new JsonObject
        {
            ["url"] = JsonValue.Create(Url)
        };

        if (!string.IsNullOrWhiteSpace(MimeType))
        {
            image["mime_type"] = JsonValue.Create(MimeType);
        }

        payload["image_url"] = image;
        return payload;
    }
}

public sealed class ChatGenericPart : ChatMessagePart
{
    private readonly JsonObject payload;

    public ChatGenericPart(JsonObject payload)
        : base(ExtractType(payload))
    {
        this.payload = payload is null
            ? throw new ArgumentNullException(nameof(payload))
            : (JsonObject)payload.DeepClone();
    }

    public JsonObject Payload => (JsonObject)payload.DeepClone();

    public override JsonObject ToJson()
        => (JsonObject)payload.DeepClone();

    private static string ExtractType(JsonObject payload)
    {
        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (!payload.TryGetPropertyValue("type", out var typeNode) || typeNode is null)
        {
            throw new ArgumentException("Generic chat message parts must include a 'type' property.", nameof(payload));
        }

        if (typeNode is JsonValue value && value.TryGetValue<string>(out var type) && !string.IsNullOrWhiteSpace(type))
        {
            return type;
        }

        throw new ArgumentException("Generic chat message parts must provide a non-empty string 'type' property.", nameof(payload));
    }
}
