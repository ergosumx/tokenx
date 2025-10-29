namespace ErgoX.TokenX.HuggingFace.Chat;

using System;
using System.Text.Json.Nodes;

/// <summary>
/// Base class for multimodal message parts in a chat message.
/// </summary>
/// <remarks>
/// Chat message parts allow representing different types of content (text, images, etc.) in a structured way.
/// Derived classes include <see cref="ChatTextPart"/>, <see cref="ChatImageUrlPart"/>, and <see cref="ChatGenericPart"/>.
/// </remarks>
public abstract class ChatMessagePart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatMessagePart"/> class.
    /// </summary>
    /// <param name="type">The part type identifier (e.g., "text", "image_url"). Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is null, empty, or whitespace.</exception>
    protected ChatMessagePart(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Part type must be provided.", nameof(type));
        }

        Type = type;
    }

    /// <summary>
    /// Gets the part type identifier (e.g., "text", "image_url").
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Creates a JSON object with the specified type.
    /// </summary>
    /// <param name="type">The type value to include in the JSON.</param>
    /// <returns>A new JSON object with the type property set.</returns>
    protected static JsonObject CreateBaseObject(string type)
    {
        var payload = new JsonObject
        {
            ["type"] = JsonValue.Create(type)
        };
        return payload;
    }

    /// <summary>
    /// Serializes the message part to JSON.
    /// </summary>
    /// <returns>The JSON representation of this part.</returns>
    public abstract JsonObject ToJson();
}

/// <summary>
/// Represents a text content part in a multimodal chat message.
/// </summary>
public sealed class ChatTextPart : ChatMessagePart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatTextPart"/> class.
    /// </summary>
    /// <param name="text">The text content. Defaults to empty string if null.</param>
    public ChatTextPart(string text)
        : base("text")
    {
        Text = text ?? string.Empty;
    }

    /// <summary>
    /// Gets the text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Serializes the text part to JSON.
    /// </summary>
    /// <returns>A JSON object with type "text" and the text content.</returns>
    public override JsonObject ToJson()
    {
        var payload = CreateBaseObject(Type);
        payload["text"] = JsonValue.Create(Text);
        return payload;
    }
}

/// <summary>
/// Represents an image URL content part in a multimodal chat message.
/// </summary>
public sealed class ChatImageUrlPart : ChatMessagePart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatImageUrlPart"/> class.
    /// </summary>
    /// <param name="url">The image URL (HTTP/HTTPS or data URL). Must not be null or whitespace.</param>
    /// <param name="mimeType">Optional MIME type of the image (e.g., "image/png", "image/jpeg").</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is null, empty, or whitespace.</exception>
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

    /// <summary>
    /// Gets the image URL.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the optional MIME type of the image.
    /// </summary>
    public string? MimeType { get; }

    /// <summary>
    /// Serializes the image URL part to JSON.
    /// </summary>
    /// <returns>A JSON object with type "image_url" and the image URL details.</returns>
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

/// <summary>
/// Represents a generic or custom message part with arbitrary JSON structure.
/// </summary>
/// <remarks>
/// This class allows representing message parts that don't fit the standard text or image patterns,
/// enabling forward compatibility with new part types from chat models.
/// </remarks>
public sealed class ChatGenericPart : ChatMessagePart
{
    private readonly JsonObject payload;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatGenericPart"/> class.
    /// </summary>
    /// <param name="payload">The JSON payload representing the message part. Must include a "type" string property.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the payload does not include a valid "type" property.</exception>
    public ChatGenericPart(JsonObject payload)
        : base(ExtractType(payload))
    {
        this.payload = payload is null
            ? throw new ArgumentNullException(nameof(payload))
            : (JsonObject)payload.DeepClone();
    }

    /// <summary>
    /// Gets a deep clone of the underlying JSON payload.
    /// </summary>
    public JsonObject Payload => (JsonObject)payload.DeepClone();

    /// <summary>
    /// Serializes the message part to JSON.
    /// </summary>
    /// <returns>A deep clone of the underlying JSON payload.</returns>
    public override JsonObject ToJson()
        => (JsonObject)payload.DeepClone();

    /// <summary>
    /// Extracts the type property from a JSON payload.
    /// </summary>
    /// <param name="payload">The JSON object.</param>
    /// <returns>The type value as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the payload does not have a valid type property.</exception>
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

