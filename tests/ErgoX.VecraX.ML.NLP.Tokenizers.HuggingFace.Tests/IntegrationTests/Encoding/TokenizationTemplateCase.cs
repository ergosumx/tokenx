namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

public sealed record TokenizationTemplateCase(
    string Id,
    string Length,
    string Description,
    string Text,
    string? PairText,
    string FilePath)
{
    public static IReadOnlyList<TokenizationTemplateCase> LoadAllForTarget(string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        var root = TestDataPath.GetTokenizationTemplatesRoot();
        if (!Directory.Exists(root))
        {
            throw new InvalidOperationException($"Tokenization templates directory missing at '{root}'.");
        }

        var files = Directory.GetFiles(root, "tokenization-*.json", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var cases = new List<TokenizationTemplateCase>();
        foreach (var filePath in files)
        {
            var json = File.ReadAllText(filePath);
            using JsonDocument document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;

            if (!ShouldInclude(rootElement, target))
            {
                continue;
            }

            var id = GetRequiredString(rootElement, "id", filePath);
            var length = GetRequiredString(rootElement, "length", filePath);
            var description = GetRequiredString(rootElement, "description", filePath);

            if (!rootElement.TryGetProperty("single", out var singleElement) || singleElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"Template '{filePath}' is missing a valid 'single' object.");
            }

            var text = GetRequiredString(singleElement, "text", filePath);
            var pairText = singleElement.TryGetProperty("pairText", out var pairElement) && pairElement.ValueKind == JsonValueKind.String
                ? pairElement.GetString()
                : null;

            cases.Add(new TokenizationTemplateCase(id, length, description, text, pairText, filePath));
        }

        if (cases.Count == 0)
        {
            throw new InvalidOperationException($"No tokenization templates available for target '{target}'.");
        }

        return cases;
    }

    private static bool ShouldInclude(JsonElement root, string target)
    {
        if (!root.TryGetProperty("targets", out var targetsElement) || targetsElement.ValueKind != JsonValueKind.Array)
        {
            return true;
        }

        using var enumerator = targetsElement.EnumerateArray();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            if (element.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var candidate = element.GetString();
            if (candidate is null)
            {
                continue;
            }

            if (candidate.Equals("*", StringComparison.OrdinalIgnoreCase) ||
                candidate.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetRequiredString(JsonElement element, string propertyName, string filePath)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Template '{0}' is missing required '{1}' property.", filePath, propertyName));
        }

        var result = value.GetString();
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Template '{0}' contains an empty '{1}' value.", filePath, propertyName));
        }

        return result;
    }
}
