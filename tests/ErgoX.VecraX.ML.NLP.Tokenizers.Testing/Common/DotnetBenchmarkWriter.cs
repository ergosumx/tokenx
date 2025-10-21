namespace ErgoX.VecraX.ML.NLP.Tokenizers.Parity;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

public static class DotnetBenchmarkWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Write(
        string destinationPath,
        PythonBenchmarkModel reference,
        IReadOnlyList<DotnetBenchmarkCaseSnapshot> cases)
    {
        if (destinationPath is null)
        {
            throw new ArgumentNullException(nameof(destinationPath));
        }

        if (reference is null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        if (cases is null)
        {
            throw new ArgumentNullException(nameof(cases));
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = new DotnetBenchmarkModelSnapshot
        {
            Model = reference.Metadata.Model,
            DisplayName = reference.Metadata.DisplayName,
            GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            TokenizerAssemblyVersion = typeof(Tokenizer).Assembly.GetName().Version?.ToString() ?? "unknown",
            PythonFixtureGeneratedAt = reference.Metadata.GeneratedAt,
            PythonTokenizersVersion = reference.Metadata.TokenizersVersion,
            Cases = cases
        };

        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        if (!File.Exists(destinationPath) || !string.Equals(File.ReadAllText(destinationPath), json, StringComparison.Ordinal))
        {
            File.WriteAllText(destinationPath, json);
        }
    }

    public static DotnetBenchmarkCaseSnapshot CreateCaseSnapshot(
        PythonBenchmarkCase testCase,
        EncodingResult singleEncoding,
        string singleDecoded,
        IReadOnlyList<string> batchTexts,
        IReadOnlyList<string>? batchPairTexts,
        IReadOnlyList<EncodingResult> batchEncodings,
        IReadOnlyList<string> batchDecoded)
    {
        if (testCase is null)
        {
            throw new ArgumentNullException(nameof(testCase));
        }

        if (singleEncoding is null)
        {
            throw new ArgumentNullException(nameof(singleEncoding));
        }

        if (singleDecoded is null)
        {
            throw new ArgumentNullException(nameof(singleDecoded));
        }

        if (batchTexts is null)
        {
            throw new ArgumentNullException(nameof(batchTexts));
        }

        if (batchEncodings is null)
        {
            throw new ArgumentNullException(nameof(batchEncodings));
        }

        if (batchDecoded is null)
        {
            throw new ArgumentNullException(nameof(batchDecoded));
        }

        return new DotnetBenchmarkCaseSnapshot
        {
            Length = testCase.Length,
            Description = testCase.Description,
            Options = testCase.Options,
            Single = new DotnetBenchmarkSingleSnapshot
            {
                Text = testCase.Single.Text,
                PairText = testCase.Single.PairText,
                Encoding = DotnetEncodingSnapshot.FromEncoding(singleEncoding),
                Decoded = singleDecoded
            },
            Batch = new DotnetBenchmarkBatchSnapshot
            {
                Texts = batchTexts.ToArray(),
                PairTexts = batchPairTexts?.ToArray(),
                Encodings = batchEncodings.Select(DotnetEncodingSnapshot.FromEncoding).ToArray(),
                Decoded = batchDecoded.ToArray()
            }
        };
    }
}

public sealed record DotnetBenchmarkModelSnapshot
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("generatedAtUtc")]
    public required string GeneratedAtUtc { get; init; }

    [JsonPropertyName("tokenizerAssemblyVersion")]
    public required string TokenizerAssemblyVersion { get; init; }

    [JsonPropertyName("pythonFixtureGeneratedAt")]
    public required string PythonFixtureGeneratedAt { get; init; }

    [JsonPropertyName("pythonTokenizersVersion")]
    public required string PythonTokenizersVersion { get; init; }

    [JsonPropertyName("cases")]
    public required IReadOnlyList<DotnetBenchmarkCaseSnapshot> Cases { get; init; }
}

public sealed record DotnetBenchmarkCaseSnapshot
{
    [JsonPropertyName("length")]
    public required string Length { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("options")]
    public required PythonBenchmarkCaseOptions Options { get; init; }

    [JsonPropertyName("single")]
    public required DotnetBenchmarkSingleSnapshot Single { get; init; }

    [JsonPropertyName("batch")]
    public required DotnetBenchmarkBatchSnapshot Batch { get; init; }
}

public sealed record DotnetBenchmarkSingleSnapshot
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("pairText")]
    public string? PairText { get; init; }

    [JsonPropertyName("encoding")]
    public required DotnetEncodingSnapshot Encoding { get; init; }

    [JsonPropertyName("decoded")]
    public required string Decoded { get; init; }
}

public sealed record DotnetBenchmarkBatchSnapshot
{
    [JsonPropertyName("texts")]
    public required IReadOnlyList<string> Texts { get; init; }

    [JsonPropertyName("pairTexts")]
    public IReadOnlyList<string>? PairTexts { get; init; }

    [JsonPropertyName("encodings")]
    public required IReadOnlyList<DotnetEncodingSnapshot> Encodings { get; init; }

    [JsonPropertyName("decoded")]
    public required IReadOnlyList<string> Decoded { get; init; }
}

public sealed record DotnetEncodingSnapshot
{
    [JsonPropertyName("length")]
    public required int Length { get; init; }

    [JsonPropertyName("ids")]
    public required IReadOnlyList<int> Ids { get; init; }

    [JsonPropertyName("tokens")]
    public required IReadOnlyList<string> Tokens { get; init; }

    [JsonPropertyName("typeIds")]
    public required IReadOnlyList<uint> TypeIds { get; init; }

    [JsonPropertyName("attentionMask")]
    public required IReadOnlyList<uint> AttentionMask { get; init; }

    [JsonPropertyName("specialTokensMask")]
    public required IReadOnlyList<uint> SpecialTokensMask { get; init; }

    [JsonPropertyName("offsets")]
    public required IReadOnlyList<DotnetEncodingOffset> Offsets { get; init; }

    [JsonPropertyName("wordIds")]
    public required IReadOnlyList<int?> WordIds { get; init; }

    [JsonPropertyName("sequenceIds")]
    public required IReadOnlyList<int?> SequenceIds { get; init; }

    [JsonPropertyName("overflowing")]
    public required IReadOnlyList<DotnetEncodingSnapshot> Overflowing { get; init; }

    public static DotnetEncodingSnapshot FromEncoding(EncodingResult encoding)
    {
        if (encoding is null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }

        return new DotnetEncodingSnapshot
        {
            Length = encoding.Length,
            Ids = encoding.Ids.ToArray(),
            Tokens = encoding.Tokens.ToArray(),
            TypeIds = encoding.TypeIds.ToArray(),
            AttentionMask = encoding.AttentionMask.ToArray(),
            SpecialTokensMask = encoding.SpecialTokensMask.ToArray(),
            Offsets = encoding.Offsets.Select(pair => new DotnetEncodingOffset
            {
                Start = pair.Start,
                End = pair.End
            }).ToArray(),
            WordIds = encoding.WordIds.ToArray(),
            SequenceIds = encoding.SequenceIds.ToArray(),
            Overflowing = encoding.Overflowing.Count == 0
                ? Array.Empty<DotnetEncodingSnapshot>()
                : encoding.Overflowing.Select(FromEncoding).ToArray()
        };
    }
}

public sealed record DotnetEncodingOffset
{
    [JsonPropertyName("start")]
    public required int Start { get; init; }

    [JsonPropertyName("end")]
    public required int End { get; init; }
}
