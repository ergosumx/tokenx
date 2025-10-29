namespace ErgoX.TokenX.Parity;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed record PythonBenchmarkModel
{
    [JsonPropertyName("metadata")]
    public required PythonBenchmarkMetadata Metadata { get; init; }

    [JsonPropertyName("cases")]
    public required IReadOnlyList<PythonBenchmarkCase> Cases { get; init; }
}

public sealed record PythonBenchmarkMetadata
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("repo_id")]
    public string? RepoId { get; init; }

    [JsonPropertyName("generated_at")]
    public required string GeneratedAt { get; init; }

    [JsonPropertyName("tokenizers_version")]
    public required string TokenizersVersion { get; init; }

    [JsonPropertyName("transformers_version")]
    public string? TransformersVersion { get; init; }

    [JsonPropertyName("assets")]
    public required IReadOnlyDictionary<string, string> Assets { get; init; }
}

public sealed record PythonBenchmarkCase
{
    [JsonPropertyName("contractId")]
    public string? ContractId { get; init; }

    [JsonPropertyName("length")]
    public required string Length { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("single")]
    public required PythonBenchmarkSingle Single { get; init; }

    [JsonPropertyName("batch")]
    public required PythonBenchmarkBatch Batch { get; init; }

    [JsonPropertyName("options")]
    public PythonBenchmarkCaseOptions Options { get; init; } = PythonBenchmarkCaseOptions.Default;
}

public sealed record PythonBenchmarkSingle
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("textHash")]
    public required string TextHash { get; init; }

    [JsonPropertyName("pairText")]
    public string? PairText { get; init; }

    [JsonPropertyName("pairTextHash")]
    public string? PairTextHash { get; init; }

    [JsonPropertyName("encoding")]
    public required EncodingSummary Encoding { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}

public sealed record PythonBenchmarkBatch
{
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("texts")]
    public IReadOnlyList<string> Texts { get; init; } = Array.Empty<string>();

    [JsonPropertyName("textsHash")]
    public required string TextsHash { get; init; }

    [JsonPropertyName("encodings")]
    public required IReadOnlyList<EncodingSummary> Encodings { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }

    [JsonPropertyName("pairTexts")]
    public IReadOnlyList<string>? PairTexts { get; init; }

    [JsonPropertyName("pairTextsHash")]
    public string? PairTextsHash { get; init; }
}

public sealed record PythonBenchmarkCaseOptions
{
    public static PythonBenchmarkCaseOptions Default { get; } = new();

    [JsonPropertyName("addSpecialTokens")]
    public bool AddSpecialTokens { get; init; }

    [JsonPropertyName("decodeSkipSpecialTokens")]
    public bool DecodeSkipSpecialTokens { get; init; } = true;

    [JsonPropertyName("truncation")]
    public PythonBenchmarkTruncationOptions? Truncation { get; init; }
}

public sealed record PythonBenchmarkTruncationOptions
{
    [JsonPropertyName("maxLength")]
    public required int MaxLength { get; init; }

    [JsonPropertyName("stride")]
    public required int Stride { get; init; }
}

public sealed record EncodingSummary
{
    [JsonPropertyName("length")]
    public required int Length { get; init; }

    [JsonPropertyName("idsHash")]
    public required string IdsHash { get; init; }

    [JsonPropertyName("tokensHash")]
    public required string TokensHash { get; init; }

    [JsonPropertyName("typeIdsHash")]
    public required string TypeIdsHash { get; init; }

    [JsonPropertyName("attentionMaskHash")]
    public required string AttentionMaskHash { get; init; }

    [JsonPropertyName("specialTokensMaskHash")]
    public required string SpecialTokensMaskHash { get; init; }

    [JsonPropertyName("offsetsHash")]
    public required string OffsetsHash { get; init; }

    [JsonPropertyName("wordIdsHash")]
    public required string WordIdsHash { get; init; }

    [JsonPropertyName("sequenceIdsHash")]
    public required string SequenceIdsHash { get; init; }

    [JsonPropertyName("overflowing")]
    public required IReadOnlyList<EncodingSummary> Overflowing { get; init; }
}

