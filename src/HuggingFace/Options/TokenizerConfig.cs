using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// Represents the complete tokenizer configuration loaded from tokenizer_config.json.
/// </summary>
/// <remarks>
/// This class deserializes and holds the tokenizer metadata including vocabulary, special tokens,
/// model configuration, chat templates, and default padding/truncation settings.
/// Properties match the HuggingFace tokenizer_config.json schema.
/// </remarks>
public sealed class TokenizerConfig
{
    /// <summary>
    /// Gets or sets the version of the tokenizer configuration format.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the model configuration (vocabulary and unknown token).
    /// </summary>
    [JsonPropertyName("model")]
    public SerializedModel Model { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of additional tokens added to the base vocabulary.
    /// </summary>
    [JsonPropertyName("added_tokens")]
    public List<SerializedAddedToken> AddedTokens { get; set; } = new();

    /// <summary>
    /// Gets or sets the default padding configuration.
    /// </summary>
    [JsonPropertyName("padding")]
    public SerializedPadding? Padding { get; set; }

    /// <summary>
    /// Gets or sets the default truncation configuration.
    /// </summary>
    [JsonPropertyName("truncation")]
    public SerializedTruncation? Truncation { get; set; }

    /// <summary>
    /// Gets or sets the beginning-of-sequence token string (e.g., "[CLS]").
    /// </summary>
    [JsonPropertyName("bos_token")]
    public string? BosToken { get; set; }

    /// <summary>
    /// Gets or sets the end-of-sequence token string (e.g., "[SEP]").
    /// </summary>
    [JsonPropertyName("eos_token")]
    public string? EosToken { get; set; }

    /// <summary>
    /// Gets or sets the beginning-of-sequence token ID.
    /// </summary>
    [JsonPropertyName("bos_token_id")]
    public int? BosTokenId { get; set; }

    /// <summary>
    /// Gets or sets the end-of-sequence token ID.
    /// </summary>
    [JsonPropertyName("eos_token_id")]
    public int? EosTokenId { get; set; }

    /// <summary>
    /// Gets or sets the padding token string (e.g., "[PAD]").
    /// </summary>
    [JsonPropertyName("pad_token")]
    public string? PadToken { get; set; }

    /// <summary>
    /// Gets or sets the padding token ID.
    /// </summary>
    [JsonPropertyName("pad_token_id")]
    public int? PadTokenId { get; set; }

    /// <summary>
    /// Gets or sets the maximum sequence length supported by the model.
    /// </summary>
    [JsonPropertyName("model_max_length")]
    public int? ModelMaxLength { get; set; }

    /// <summary>
    /// Gets or sets the Jinja2 chat template string for formatting messages.
    /// </summary>
    [JsonPropertyName("chat_template")]
    public string? ChatTemplate { get; set; }

    /// <summary>
    /// Gets or sets the mapping of chat roles to their template identifiers.
    /// </summary>
    [JsonPropertyName("chat_template_roles")]
    public Dictionary<string, string>? ChatTemplateRoles { get; set; }

    /// <summary>
    /// Gets or sets the vocabulary dictionary (token string to ID mapping).
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, int> Vocab
    {
        get => Model.Vocab;
        set => Model.Vocab = value;
    }

    /// <summary>
    /// Gets or sets the unknown token string used for out-of-vocabulary tokens.
    /// </summary>
    [JsonIgnore]
    public string? UnknownToken
    {
        get => Model.UnknownToken;
        set => Model.UnknownToken = value;
    }

    /// <summary>
    /// Gets the ID of the unknown token, or <c>null</c> if not found in the vocabulary.
    /// </summary>
    [JsonIgnore]
    public int? UnknownTokenId
    {
        get
        {
            if (UnknownToken is not null && Vocab.TryGetValue(UnknownToken, out var id))
            {
                return id;
            }

            return null;
        }
    }

    /// <summary>
    /// Serializes the tokenizer configuration to a JSON string.
    /// </summary>
    /// <param name="pretty">If <c>true</c>, the output is formatted for readability.</param>
    /// <returns>The serialized configuration JSON.</returns>
    public string ToJson(bool pretty)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = pretty,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Deserializes a tokenizer configuration from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing the configuration.</param>
    /// <returns>A <see cref="TokenizerConfig"/> instance parsed from the JSON.</returns>
    /// <remarks>
    /// This method handles added tokens by merging them into the vocabulary.
    /// Property names are matched case-insensitively.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the JSON cannot be parsed into a valid configuration.</exception>
    public static TokenizerConfig FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var config = JsonSerializer.Deserialize<TokenizerConfig>(json, options);
        if (config is null)
        {
            throw new InvalidOperationException("Tokenizer configuration payload could not be parsed.");
        }

        if (config.AddedTokens is { Count: > 0 })
        {
            foreach (var token in config.AddedTokens)
            {
                if (token is null || token.Id is null)
                {
                    continue;
                }

                config.Model.Vocab[token.Content] = token.Id.Value;
            }
        }

        return config;
    }

    /// <summary>
    /// Represents the model-specific vocabulary and unknown token configuration.
    /// </summary>
    public sealed class SerializedModel
    {
        /// <summary>
        /// Gets or sets the vocabulary dictionary (token string to ID mapping).
        /// </summary>
        [JsonPropertyName("vocab")]
        public Dictionary<string, int> Vocab { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the unknown token string used for out-of-vocabulary tokens.
        /// </summary>
        [JsonPropertyName("unk_token")]
        public string? UnknownToken { get; set; }
    }

    /// <summary>
    /// Represents a single added token with its properties.
    /// </summary>
    public sealed class SerializedAddedToken
    {
        /// <summary>
        /// Gets or sets the token ID.
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the token string content.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the token should only match as a single word (not as a subword).
        /// </summary>
        [JsonPropertyName("single_word")]
        public bool SingleWord { get; set; }

        /// <summary>
        /// Gets or sets whether leading whitespace should be stripped during tokenization.
        /// </summary>
        [JsonPropertyName("lstrip")]
        public bool LeftStrip { get; set; }

        /// <summary>
        /// Gets or sets whether trailing whitespace should be stripped during tokenization.
        /// </summary>
        [JsonPropertyName("rstrip")]
        public bool RightStrip { get; set; }

        /// <summary>
        /// Gets or sets whether the token content should be normalized before matching.
        /// </summary>
        [JsonPropertyName("normalized")]
        public bool Normalized { get; set; }

        /// <summary>
        /// Gets or sets whether the token is a special token.
        /// </summary>
        [JsonPropertyName("special")]
        public bool Special { get; set; }
    }

    /// <summary>
    /// Represents the default padding configuration from tokenizer_config.json.
    /// </summary>
    public sealed class SerializedPadding
    {
        /// <summary>
        /// Gets or sets the padding direction ("left" or "right").
        /// </summary>
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "right";

        /// <summary>
        /// Gets or sets the token ID used for padding.
        /// </summary>
        [JsonPropertyName("pad_id")]
        public int PadId { get; set; }

        /// <summary>
        /// Gets or sets the type ID (segment ID) for padding tokens.
        /// </summary>
        [JsonPropertyName("pad_type_id")]
        public int PadTypeId { get; set; }

        /// <summary>
        /// Gets or sets the token string used for padding.
        /// </summary>
        [JsonPropertyName("pad_token")]
        public string PadToken { get; set; } = "[PAD]";

        /// <summary>
        /// Gets or sets the target padding length, or <c>null</c> to pad to the longest sequence.
        /// </summary>
        [JsonPropertyName("length")]
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the multiple to which sequence lengths should be padded, or <c>null</c> for no multiple.
        /// </summary>
        [JsonPropertyName("pad_to_multiple_of")]
        public int? PadToMultipleOf { get; set; }
    }

    /// <summary>
    /// Represents the default truncation configuration from tokenizer_config.json.
    /// </summary>
    public sealed class SerializedTruncation
    {
        /// <summary>
        /// Gets or sets the maximum sequence length before truncation.
        /// </summary>
        [JsonPropertyName("max_length")]
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the stride for sliding window truncation.
        /// </summary>
        [JsonPropertyName("stride")]
        public int Stride { get; set; }

        /// <summary>
        /// Gets or sets the truncation strategy ("longest_first", "only_first", or "only_second").
        /// </summary>
        [JsonPropertyName("strategy")]
        public string Strategy { get; set; } = "longest_first";

        /// <summary>
        /// Gets or sets the truncation direction ("left" or "right").
        /// </summary>
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "right";
    }
}

