namespace ErgoX.TokenX.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
/// <remarks>
/// A chat message consists of a role (e.g., "user", "assistant", "system"),
/// either text content or multimodal parts (text, images, etc.), and optional metadata.
/// Either <see cref="Content"/> or <see cref="Parts"/> will be populated, not both.
/// </remarks>
public sealed class ChatMessage
{
    private static readonly IReadOnlyList<ChatMessagePart> EmptyParts = Array.Empty<ChatMessagePart>();
    private static readonly IReadOnlyDictionary<string, JsonNode?> EmptyMetadata =
        new ReadOnlyDictionary<string, JsonNode?>(
            new Dictionary<string, JsonNode?>(StringComparer.Ordinal));

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatMessage"/> class.
    /// </summary>
    /// <param name="role">The message role (e.g., "user", "assistant", "system"). Must not be null or whitespace.</param>
    /// <param name="content">The text message content (mutually exclusive with <paramref name="parts"/>).</param>
    /// <param name="parts">Multimodal message parts like text and images (mutually exclusive with <paramref name="content"/>).</param>
    /// <param name="metadata">Optional custom metadata associated with the message.</param>
    /// <exception cref="ArgumentException">Thrown when role is null/empty, both content and parts are provided, or parts list is empty.</exception>
    public ChatMessage(
        string role,
        string? content,
        IReadOnlyList<ChatMessagePart>? parts,
        IReadOnlyDictionary<string, JsonNode?>? metadata)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Message role must be provided.", nameof(role));
        }

        if (parts is not null && parts.Count == 0)
        {
            throw new ArgumentException("At least one chat message part must be provided.", nameof(parts));
        }

        if (parts is not null && content is not null)
        {
            throw new ArgumentException("Provide either message content text or content parts, not both.", nameof(parts));
        }

        Role = role;
        Parts = NormalizeParts(parts);
        Content = Parts.Count > 0 ? null : content ?? string.Empty;
        Metadata = BuildMetadata(metadata);
    }

    /// <summary>
    /// Gets the message role (e.g., "user", "assistant", "system").
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Gets the text content of the message, or <c>null</c> if the message uses multimodal parts instead.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the multimodal parts of the message (text, images, etc.), or an empty list if the message has text content instead.
    /// </summary>
    public IReadOnlyList<ChatMessagePart> Parts { get; }

    /// <summary>
    /// Gets optional custom metadata associated with the message.
    /// </summary>
    public IReadOnlyDictionary<string, JsonNode?> Metadata { get; }

    /// <summary>
    /// Creates a chat message with text content.
    /// </summary>
    /// <param name="role">The message role.</param>
    /// <param name="content">The text content.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A <see cref="ChatMessage"/> with the specified role and text content.</returns>
    public static ChatMessage FromText(string role, string content, IReadOnlyDictionary<string, JsonNode?>? metadata = null)
        => new(role, content ?? string.Empty, null, metadata);

    /// <summary>
    /// Creates a chat message with multimodal parts.
    /// </summary>
    /// <param name="role">The message role.</param>
    /// <param name="parts">The message parts (text, images, etc.).</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A <see cref="ChatMessage"/> with the specified role and multimodal parts.</returns>
    public static ChatMessage FromParts(string role, IEnumerable<ChatMessagePart> parts, IReadOnlyDictionary<string, JsonNode?>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(parts);
        var materialized = parts as ChatMessagePart[] ?? parts.ToArray();
        return new ChatMessage(role, null, materialized, metadata);
    }

    private static IReadOnlyList<ChatMessagePart> NormalizeParts(IReadOnlyList<ChatMessagePart>? parts)
    {
        if (parts is null)
        {
            return EmptyParts;
        }

        return parts is ChatMessagePart[] array
            ? Array.AsReadOnly(array)
            : new ReadOnlyCollection<ChatMessagePart>(parts.ToArray());
    }

    private static IReadOnlyDictionary<string, JsonNode?> BuildMetadata(IReadOnlyDictionary<string, JsonNode?>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return EmptyMetadata;
        }

        var buffer = new Dictionary<string, JsonNode?>(metadata.Count, StringComparer.Ordinal);
        foreach (var pair in metadata)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            buffer[pair.Key] = pair.Value?.DeepClone();
        }

        return buffer.Count == 0
            ? EmptyMetadata
            : new ReadOnlyDictionary<string, JsonNode?>(buffer);
    }
}

