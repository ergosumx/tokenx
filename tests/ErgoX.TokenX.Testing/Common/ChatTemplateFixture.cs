namespace ErgoX.TokenX.Parity;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ChatTemplateFixture
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("repo_id")]
    public string? RepoId { get; set; }
    [JsonPropertyName("cases")]
    public List<ChatTemplateCase> Cases { get; set; } = new();
}

public sealed class ChatTemplateCase
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("addGenerationPrompt")]
    public bool AddGenerationPrompt { get; set; } = true;

    [JsonPropertyName("messages")]
    public List<ChatTemplateMessage> Messages { get; set; } = new();

    [JsonPropertyName("rendered")]
    public string Rendered { get; set; } = string.Empty;

    [JsonPropertyName("renderedHash")]
    public string RenderedHash { get; set; } = string.Empty;

    [JsonPropertyName("tokenIds")]
    public List<int> TokenIds { get; set; } = new();

    [JsonPropertyName("tokenIdsHash")]
    public string TokenIdsHash { get; set; } = string.Empty;
}

public sealed class ChatTemplateMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new(StringComparer.Ordinal);
}

