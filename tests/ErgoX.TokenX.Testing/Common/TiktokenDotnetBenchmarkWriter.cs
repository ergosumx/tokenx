namespace ErgoX.TokenX.Parity;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class TiktokenDotnetBenchmarkWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Write(
        string destinationPath,
    TiktokenDotnetBenchmarkMetadata metadata,
    IReadOnlyList<TiktokenDotnetBenchmarkCaseSnapshot> cases,
    string dotnetAssemblyVersion)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("Destination path must be provided.", nameof(destinationPath));
        }

        if (metadata is null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        if (cases is null)
        {
            throw new ArgumentNullException(nameof(cases));
        }

        if (metadata.SpecialTokens is null)
        {
            throw new ArgumentException("Special tokens metadata must be provided.", nameof(metadata));
        }

        if (string.IsNullOrWhiteSpace(dotnetAssemblyVersion))
        {
            throw new ArgumentException("Assembly version must be provided.", nameof(dotnetAssemblyVersion));
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = new TiktokenDotnetBenchmarkModelSnapshot
        {
            Encoding = metadata.Encoding,
            DisplayName = string.IsNullOrWhiteSpace(metadata.DisplayName) ? metadata.Encoding : metadata.DisplayName,
            Model = string.IsNullOrWhiteSpace(metadata.Model) ? metadata.Encoding : metadata.Model,
            GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            DotnetAssemblyVersion = dotnetAssemblyVersion,
            PythonFixtureGeneratedAt = metadata.GeneratedAt,
            PythonTiktokenVersion = metadata.TiktokenVersion,
            Pattern = metadata.Pattern,
            MergeableRanksFile = metadata.MergeableRanksFile,
            ExplicitVocabularySize = metadata.ExplicitVocabularySize,
            PythonVocabularySize = metadata.PythonVocabularySize,
            SpecialTokens = new SortedDictionary<string, int>(
                metadata.SpecialTokens.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                StringComparer.Ordinal),
            Cases = cases.ToArray()
        };

        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        if (!File.Exists(destinationPath) || !string.Equals(File.ReadAllText(destinationPath), json, StringComparison.Ordinal))
        {
            File.WriteAllText(destinationPath, json);
        }
    }
}

public sealed record TiktokenDotnetBenchmarkMetadata
{
    public required string Encoding { get; init; }

    public required string DisplayName { get; init; }

    public required string Model { get; init; }

    public required string GeneratedAt { get; init; }

    public required string TiktokenVersion { get; init; }

    public required string Pattern { get; init; }

    public required string MergeableRanksFile { get; init; }

    public required IReadOnlyDictionary<string, int> SpecialTokens { get; init; }

    public required int ExplicitVocabularySize { get; init; }

    public int? PythonVocabularySize { get; init; }
}

public sealed record TiktokenDotnetBenchmarkModelSnapshot
{
    [JsonPropertyName("encoding")]
    public required string Encoding { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("generatedAtUtc")]
    public required string GeneratedAtUtc { get; init; }

    [JsonPropertyName("dotnetAssemblyVersion")]
    public required string DotnetAssemblyVersion { get; init; }

    [JsonPropertyName("pythonFixtureGeneratedAt")]
    public required string PythonFixtureGeneratedAt { get; init; }

    [JsonPropertyName("pythonTiktokenVersion")]
    public required string PythonTiktokenVersion { get; init; }

    [JsonPropertyName("pattern")]
    public required string Pattern { get; init; }

    [JsonPropertyName("mergeableRanksFile")]
    public required string MergeableRanksFile { get; init; }

    [JsonPropertyName("explicitVocabularySize")]
    public required int ExplicitVocabularySize { get; init; }

    [JsonPropertyName("pythonVocabularySize")]
    public int? PythonVocabularySize { get; init; }

    [JsonPropertyName("specialTokens")]
    public required IReadOnlyDictionary<string, int> SpecialTokens { get; init; }

    [JsonPropertyName("cases")]
    public required IReadOnlyList<TiktokenDotnetBenchmarkCaseSnapshot> Cases { get; init; }
}

public sealed record TiktokenDotnetBenchmarkCaseSnapshot
{
    [JsonPropertyName("contractId")]
    public string? ContractId { get; init; }

    [JsonPropertyName("length")]
    public string? Length { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("single")]
    public required TiktokenDotnetBenchmarkSingleSnapshot Single { get; init; }

    [JsonPropertyName("batch")]
    public required TiktokenDotnetBenchmarkBatchSnapshot Batch { get; init; }
}

public sealed record TiktokenDotnetBenchmarkSingleSnapshot
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("pairText")]
    public string? PairText { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }

    [JsonPropertyName("tokenIds")]
    public required IReadOnlyList<int> TokenIds { get; init; }

    [JsonPropertyName("idsHash")]
    public required string IdsHash { get; init; }

    [JsonPropertyName("decoded")]
    public required string Decoded { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}

public sealed record TiktokenDotnetBenchmarkBatchSnapshot
{
    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("texts")]
    public required IReadOnlyList<string> Texts { get; init; }

    [JsonPropertyName("textsHash")]
    public required string TextsHash { get; init; }

    [JsonPropertyName("encodings")]
    public required IReadOnlyList<TiktokenDotnetEncodingSnapshot> Encodings { get; init; }

    [JsonPropertyName("decoded")]
    public required IReadOnlyList<string> Decoded { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}

public sealed record TiktokenDotnetEncodingSnapshot
{
    [JsonPropertyName("length")]
    public required int Length { get; init; }

    [JsonPropertyName("tokenIds")]
    public required IReadOnlyList<int> TokenIds { get; init; }

    [JsonPropertyName("idsHash")]
    public required string IdsHash { get; init; }

    [JsonPropertyName("decodedHash")]
    public required string DecodedHash { get; init; }
}

