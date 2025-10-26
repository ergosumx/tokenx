namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.IntegrationTests.Templates;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Parity;
using Xunit;

internal static class TemplateTestUtilities
{
    private static readonly JsonSerializerOptions TemplateSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions SummarySerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static void AssertTemplateCase(string modelFolder, string templateFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateFileName);

        var template = LoadTemplate(templateFileName);
        var baseline = LoadBaseline(modelFolder, template.Id);

        using var tokenizer = CreateTokenizer(modelFolder);

        tokenizer.DisablePadding();
        tokenizer.DisableTruncation();

        var encoding = template.PairText is null
            ? tokenizer.Encode(template.Text)
            : tokenizer.Encode(template.Text, template.PairText);

        var actualTextHash = ParityHashUtilities.HashString(template.Text);
        Assert.Equal(baseline.TextHash, actualTextHash);

        var summary = ParityHashUtilities.CreateSummary(encoding);
        var summaryJson = JsonSerializer.Serialize(summary, SummarySerializerOptions);
        var actualEncodingHash = ComputeSha256(summaryJson);
        Assert.Equal(baseline.EncodingHash, actualEncodingHash);
    }

    private static Tokenizer CreateTokenizer(string modelFolder)
    {
        var tokenizerPath = TestDataPath.GetModelTokenizerPath(modelFolder);
        if (!File.Exists(tokenizerPath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Tokenizer file '{0}' was not found.", tokenizerPath), tokenizerPath);
        }

        var json = File.ReadAllText(tokenizerPath);
        return new Tokenizer(json);
    }

    private static TemplateDefinition LoadTemplate(string templateFileName)
    {
        var templatePath = Path.Combine(TestDataPath.GetTokenizationTemplatesRoot(), templateFileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Template file '{0}' was not found.", templatePath), templatePath);
        }

        var payload = JsonSerializer.Deserialize<TemplatePayload>(File.ReadAllText(templatePath), TemplateSerializerOptions);
        if (payload is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Template '{0}' could not be deserialized.", templateFileName));
        }

        if (payload.Single is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Template '{0}' is missing a 'single' definition.", templateFileName));
        }

        if (string.IsNullOrWhiteSpace(payload.Id))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Template '{0}' does not define an 'id'.", templateFileName));
        }

        if (string.IsNullOrWhiteSpace(payload.Single.Text))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Template '{0}' single.text value is empty.", templateFileName));
        }

        return new TemplateDefinition(payload.Id, payload.Single.Text, payload.Single.PairText);
    }

    private static TemplateBaseline LoadBaseline(string modelFolder, string templateId)
    {
        var manifestPath = TestDataPath.GetModelValidationManifestPath(modelFolder);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Validation manifest '{0}' was not found.", manifestPath), manifestPath);
        }

        var manifest = JsonSerializer.Deserialize<ValidationManifest>(File.ReadAllText(manifestPath), TemplateSerializerOptions);
        if (manifest?.Cases is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Validation manifest '{0}' does not contain a 'cases' object.", manifestPath));
        }

        if (!manifest.Cases.TryGetValue(templateId, out var entry) || entry?.Python is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Validation manifest '{0}' does not define case '{1}'.", manifestPath, templateId));
        }

        var python = entry.Python;
        if (string.IsNullOrWhiteSpace(python.TextHash) || string.IsNullOrWhiteSpace(python.EncodingHash))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Validation manifest '{0}' case '{1}' contains empty hash values.", manifestPath, templateId));
        }

        return new TemplateBaseline(python.TextHash, python.EncodingHash);
    }

    private static string ComputeSha256(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed record TemplateDefinition(string Id, string Text, string? PairText);

    private sealed record TemplatePayload
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("single")]
        public TemplateSinglePayload? Single { get; init; }
    }

    private sealed record TemplateSinglePayload
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }

        [JsonPropertyName("pairText")]
        public string? PairText { get; init; }
    }

    private sealed record ValidationManifest
    {
        [JsonPropertyName("cases")]
        public Dictionary<string, ValidationCase?>? Cases { get; init; }
    }

    private sealed record ValidationCase
    {
        [JsonPropertyName("py")]
        public ValidationSnapshot? Python { get; init; }
    }

    private sealed record ValidationSnapshot
    {
        [JsonPropertyName("text-hash")]
        public string? TextHash { get; init; }

        [JsonPropertyName("encoding-hash")]
        public string? EncodingHash { get; init; }
    }

    private readonly record struct TemplateBaseline(string TextHash, string EncodingHash);
}
