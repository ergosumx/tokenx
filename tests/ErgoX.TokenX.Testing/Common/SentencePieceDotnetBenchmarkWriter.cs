namespace ErgoX.TokenX.Parity;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
public static class SentencePieceDotnetBenchmarkWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Write(
        string destinationPath,
        PythonBenchmarkModel reference,
        IReadOnlyList<DotnetBenchmarkCaseSnapshot> cases,
        string processorAssemblyVersion)
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

        if (string.IsNullOrWhiteSpace(processorAssemblyVersion))
        {
            throw new ArgumentException("Assembly version is required.", nameof(processorAssemblyVersion));
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = new SentencePieceDotnetBenchmarkModelSnapshot
        {
            Model = reference.Metadata.Model,
            DisplayName = reference.Metadata.DisplayName,
            GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ProcessorAssemblyVersion = processorAssemblyVersion,
            PythonFixtureGeneratedAt = reference.Metadata.GeneratedAt,
            PythonSentencePieceVersion = reference.Metadata.TokenizersVersion,
            Cases = cases
        };

        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        if (!File.Exists(destinationPath) || !string.Equals(File.ReadAllText(destinationPath), json, StringComparison.Ordinal))
        {
            File.WriteAllText(destinationPath, json);
        }
    }
}

public sealed record SentencePieceDotnetBenchmarkModelSnapshot
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("generatedAtUtc")]
    public required string GeneratedAtUtc { get; init; }

    [JsonPropertyName("processorAssemblyVersion")]
    public required string ProcessorAssemblyVersion { get; init; }

    [JsonPropertyName("pythonFixtureGeneratedAt")]
    public required string PythonFixtureGeneratedAt { get; init; }

    [JsonPropertyName("pythonSentencePieceVersion")]
    public required string PythonSentencePieceVersion { get; init; }

    [JsonPropertyName("cases")]
    public required IReadOnlyList<DotnetBenchmarkCaseSnapshot> Cases { get; init; }
}

