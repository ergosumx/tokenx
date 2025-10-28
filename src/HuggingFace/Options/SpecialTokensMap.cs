namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the special tokens map loaded from special_tokens_map.json.
/// </summary>
/// <remarks>
/// This class holds definitions for special tokens used by the tokenizer, such as padding, unknown, and begin/end-of-sequence tokens.
/// Each token can be defined as a simple string or as a structured object with ID and content.
/// </remarks>
public sealed class SpecialTokensMap
{
    /// <summary>
    /// Gets or sets the beginning-of-sequence token definition.
    /// </summary>
    [JsonPropertyName("bos_token")]
    public TokenDefinition? BosToken { get; set; }

    /// <summary>
    /// Gets or sets the end-of-sequence token definition.
    /// </summary>
    [JsonPropertyName("eos_token")]
    public TokenDefinition? EosToken { get; set; }

    /// <summary>
    /// Gets or sets the unknown token definition (for out-of-vocabulary tokens).
    /// </summary>
    [JsonPropertyName("unk_token")]
    public TokenDefinition? UnknownToken { get; set; }

    /// <summary>
    /// Gets or sets the padding token definition.
    /// </summary>
    [JsonPropertyName("pad_token")]
    public TokenDefinition? PadToken { get; set; }

    /// <summary>
    /// Gets or sets the list of additional special tokens beyond the standard four.
    /// </summary>
    [JsonPropertyName("additional_special_tokens")]
    public List<TokenDefinition> AdditionalSpecialTokens { get; set; } = new();

    /// <summary>
    /// Parses a special tokens map from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing the special tokens map.</param>
    /// <returns>A <see cref="SpecialTokensMap"/> instance if parsing succeeds; <c>null</c> if the JSON is null or empty.</returns>
    /// <remarks>
    /// This method is lenient and returns <c>null</c> for invalid input rather than throwing.
    /// Comments and trailing commas in JSON are supported.
    /// </remarks>
    /// <exception cref="JsonException">Thrown when the JSON is malformed in ways that cannot be recovered.</exception>
    public static SpecialTokensMap? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var root = document.RootElement;

        var result = new SpecialTokensMap
        {
            BosToken = TryParseTokenDefinition(root, "bos_token"),
            EosToken = TryParseTokenDefinition(root, "eos_token"),
            UnknownToken = TryParseTokenDefinition(root, "unk_token"),
            PadToken = TryParseTokenDefinition(root, "pad_token"),
            AdditionalSpecialTokens = TryParseTokenDefinitionList(root, "additional_special_tokens")
        };

        return result;
    }

    /// <summary>
    /// Represents a single token definition that can be a string, ID, or structured object.
    /// </summary>
    /// <remarks>
    /// A token definition may contain just an ID, just content (token string), or both.
    /// This flexibility allows compatibility with various HuggingFace tokenizer configurations.
    /// </remarks>
    public sealed class TokenDefinition
    {
        /// <summary>
        /// Gets or sets the token ID.
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the token string (e.g., "[PAD]", "[CLS]").
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private static TokenDefinition? TryParseTokenDefinition(JsonElement parent, string propertyName)
    {
        if (!TryGetPropertyCaseInsensitive(parent, propertyName, out var element))
        {
            return null;
        }

        return ParseTokenDefinition(element);
    }

    private static List<TokenDefinition> TryParseTokenDefinitionList(JsonElement parent, string propertyName)
    {
        if (!TryGetPropertyCaseInsensitive(parent, propertyName, out var element))
        {
            return new List<TokenDefinition>();
        }

        if (element.ValueKind == JsonValueKind.Null)
        {
            return new List<TokenDefinition>();
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            var single = ParseTokenDefinition(element);
            return single is null ? new List<TokenDefinition>() : new List<TokenDefinition> { single };
        }

        var results = new List<TokenDefinition>();

        using var enumerator = element.EnumerateArray();
        while (enumerator.MoveNext())
        {
            var token = ParseTokenDefinition(enumerator.Current);
            if (token is not null)
            {
                results.Add(token);
            }
        }

        return results;
    }

    private static TokenDefinition? ParseTokenDefinition(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Undefined => null,
        JsonValueKind.Null => null,
        JsonValueKind.String => new TokenDefinition { Content = element.GetString() },
        JsonValueKind.Number => element.TryGetInt32(out var id)
            ? new TokenDefinition { Id = id }
            : throw new JsonException("Token id must fit in a 32-bit signed integer."),
        JsonValueKind.Object => new TokenDefinition
        {
            Id = TryReadInt32(element, "id") ?? TryReadInt32(element, "token_id"),
            Content = TryReadString(element, "content") ?? TryReadString(element, "token")
        },
        _ => throw new JsonException($"Unsupported token definition JSON token '{element.ValueKind}'.")
    };

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        using var enumerator = element.EnumerateObject();
        while (enumerator.MoveNext())
        {
            var property = enumerator.Current;
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = property.Value;
            return true;
        }

        value = default;
        return false;
    }

    private static int? TryReadInt32(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyCaseInsensitive(element, propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numericValue))
        {
            return numericValue;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue))
        {
            return stringValue;
        }

        return null;
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyCaseInsensitive(element, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }
}
