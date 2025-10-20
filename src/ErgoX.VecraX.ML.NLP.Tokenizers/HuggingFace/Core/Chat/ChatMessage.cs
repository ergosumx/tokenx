namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;

public sealed class ChatMessage
{
    private static readonly IReadOnlyList<ChatMessagePart> EmptyParts = Array.Empty<ChatMessagePart>();
    private static readonly IReadOnlyDictionary<string, JsonNode?> EmptyMetadata =
        new ReadOnlyDictionary<string, JsonNode?>(
            new Dictionary<string, JsonNode?>(StringComparer.Ordinal));

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

    public string Role { get; }

    public string? Content { get; }

    public IReadOnlyList<ChatMessagePart> Parts { get; }

    public IReadOnlyDictionary<string, JsonNode?> Metadata { get; }

    public static ChatMessage FromText(string role, string content, IReadOnlyDictionary<string, JsonNode?>? metadata = null)
        => new(role, content ?? string.Empty, null, metadata);

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
