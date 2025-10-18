using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public sealed class SpecialTokensMap
{
    [JsonPropertyName("bos_token")]
    public TokenDefinition? BosToken { get; set; }

    [JsonPropertyName("eos_token")]
    public TokenDefinition? EosToken { get; set; }

    [JsonPropertyName("unk_token")]
    public TokenDefinition? UnknownToken { get; set; }

    [JsonPropertyName("pad_token")]
    public TokenDefinition? PadToken { get; set; }

    [JsonPropertyName("additional_special_tokens")]
    public List<TokenDefinition> AdditionalSpecialTokens { get; set; } = new();

    public static SpecialTokensMap? FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        return JsonSerializer.Deserialize<SpecialTokensMap>(json, options);
    }

    public sealed class TokenDefinition
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
