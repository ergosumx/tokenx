namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class TokenizationValidationManifest
{
    internal TokenizationValidationManifest(string model, string path, int version, ImmutableDictionary<string, TokenizationValidationCase> cases)
    {
        Model = model;
        Path = path;
        Version = version;
        Cases = cases;
    }

    public string Model { get; }

    public string Path { get; }

    public int Version { get; }

    public ImmutableDictionary<string, TokenizationValidationCase> Cases { get; }

    public static TokenizationValidationManifest Load(string model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        var path = TestDataPath.GetModelValidationManifestPath(model);
        if (!File.Exists(path))
        {
            var cases = ImmutableDictionary<string, TokenizationValidationCase>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
            return new TokenizationValidationManifest(model, path, version: 1, cases);
        }

        using var stream = File.OpenRead(path);
        var payload = JsonSerializer.Deserialize<ManifestPayload>(stream, SerializerOptions)
                      ?? throw new InvalidOperationException($"Failed to deserialize validation manifest at '{path}'.");

        var comparer = StringComparer.OrdinalIgnoreCase;
        var builder = ImmutableDictionary.CreateBuilder<string, TokenizationValidationCase>(comparer);
        if (payload.Cases is not null)
        {
            foreach (var kvp in payload.Cases)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value is null)
                {
                    continue;
                }

                builder[kvp.Key] = kvp.Value;
            }
        }

        return new TokenizationValidationManifest(payload.Model ?? model, path, payload.Version, builder.ToImmutable());
    }

    public bool TryGetCase(string caseId, out TokenizationValidationCase? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        return Cases.TryGetValue(caseId, out value);
    }

    private sealed record ManifestPayload
    {
        [JsonPropertyName("version")]
        public int Version { get; init; } = 1;

        [JsonPropertyName("model")]
        public string? Model { get; init; }

        [JsonPropertyName("cases")]
        public Dictionary<string, TokenizationValidationCase?>? Cases { get; init; }
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };
}

internal sealed record TokenizationValidationCase
{
    [JsonPropertyName("py")]
    public TokenizationValidationSnapshot? Python { get; init; }

    [JsonPropertyName("tokenx")]
    public TokenizationValidationSnapshot? Tokenx { get; init; }
}

internal sealed record TokenizationValidationSnapshot
{
    [JsonPropertyName("text-hash")]
    public string? TextHash { get; init; }

    [JsonPropertyName("encoding-hash")]
    public string? EncodingHash { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
