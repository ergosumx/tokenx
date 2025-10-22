namespace ErgoX.VecraX.ML.NLP.Tokenizers.Parity;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed record TokenizationContract
{
    [JsonPropertyName("version")]
    public int Version { get; init; }

    [JsonPropertyName("cases")]
    public required IReadOnlyList<TokenizationContractCase> Cases { get; init; }
}

public sealed record TokenizationContractCase
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("targets")]
    public IReadOnlyList<string> Targets { get; init; } = Array.Empty<string>();

    [JsonPropertyName("length")]
    public required string Length { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("options")]
    public TokenizationContractOptions Options { get; init; } = new();

    [JsonPropertyName("single")]
    public required TokenizationContractSingleCase Single { get; init; }

    [JsonPropertyName("batch")]
    public required TokenizationContractBatchCase Batch { get; init; }

    public bool AppliesToTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Target name is required.", nameof(target));
        }

        if (Targets.Count == 0)
        {
            return true;
        }

        foreach (var candidate in Targets)
        {
            if (string.Equals(candidate, "*", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(candidate, target, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

public sealed record TokenizationContractOptions
{
    [JsonPropertyName("addSpecialTokens")]
    public bool AddSpecialTokens { get; init; }

    [JsonPropertyName("decodeSkipSpecialTokens")]
    public bool DecodeSkipSpecialTokens { get; init; } = true;

    [JsonPropertyName("truncation")]
    public TokenizationContractTruncationOptions? Truncation { get; init; }
}

public sealed record TokenizationContractTruncationOptions
{
    [JsonPropertyName("maxLength")]
    public required int MaxLength { get; init; }

    [JsonPropertyName("stride")]
    public required int Stride { get; init; }
}

public sealed record TokenizationContractSingleCase
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("textHash")]
    public required string TextHash { get; init; }

    [JsonPropertyName("pairText")]
    public string? PairText { get; init; }

    [JsonPropertyName("pairTextHash")]
    public string? PairTextHash { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}

public sealed record TokenizationContractBatchCase
{
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("texts")]
    public IReadOnlyList<string> Texts { get; init; } = Array.Empty<string>();

    [JsonPropertyName("textsHash")]
    public required string TextsHash { get; init; }

    [JsonPropertyName("pairTexts")]
    public IReadOnlyList<string>? PairTexts { get; init; }

    [JsonPropertyName("pairTextsHash")]
    public string? PairTextsHash { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}
