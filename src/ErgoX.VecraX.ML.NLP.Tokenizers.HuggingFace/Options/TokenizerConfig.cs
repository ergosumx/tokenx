using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public sealed class TokenizerConfig
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("model")]
    public SerializedModel Model { get; set; } = new();

    [JsonPropertyName("added_tokens")]
    public List<SerializedAddedToken> AddedTokens { get; set; } = new();

    [JsonPropertyName("padding")]
    public SerializedPadding? Padding { get; set; }

    [JsonPropertyName("truncation")]
    public SerializedTruncation? Truncation { get; set; }

    [JsonIgnore]
    public Dictionary<string, int> Vocab
    {
        get => Model.Vocab;
        set => Model.Vocab = value;
    }

    [JsonIgnore]
    public string? UnknownToken
    {
        get => Model.UnknownToken;
        set => Model.UnknownToken = value;
    }

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

    public string ToJson(bool pretty)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = pretty,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }

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

    public sealed class SerializedModel
    {
        [JsonPropertyName("vocab")]
        public Dictionary<string, int> Vocab { get; set; } = new(StringComparer.Ordinal);

        [JsonPropertyName("unk_token")]
        public string? UnknownToken { get; set; }
    }

    public sealed class SerializedAddedToken
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("single_word")]
        public bool SingleWord { get; set; }

        [JsonPropertyName("lstrip")]
        public bool LeftStrip { get; set; }

        [JsonPropertyName("rstrip")]
        public bool RightStrip { get; set; }

        [JsonPropertyName("normalized")]
        public bool Normalized { get; set; }

        [JsonPropertyName("special")]
        public bool Special { get; set; }
    }

    public sealed class SerializedPadding
    {
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "right";

        [JsonPropertyName("pad_id")]
        public int PadId { get; set; }

        [JsonPropertyName("pad_type_id")]
        public int PadTypeId { get; set; }

        [JsonPropertyName("pad_token")]
        public string PadToken { get; set; } = "[PAD]";

        [JsonPropertyName("length")]
        public int? Length { get; set; }

        [JsonPropertyName("pad_to_multiple_of")]
        public int? PadToMultipleOf { get; set; }
    }

    public sealed class SerializedTruncation
    {
        [JsonPropertyName("max_length")]
        public int MaxLength { get; set; }

        [JsonPropertyName("stride")]
        public int Stride { get; set; }

        [JsonPropertyName("strategy")]
        public string Strategy { get; set; } = "longest_first";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "right";
    }
}
