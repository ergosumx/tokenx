namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// Maps generation configuration payloads to logits processor/warper bindings using the native bridge.
/// </summary>
internal static class LogitsBindingPlanner
{
    public static IReadOnlyList<LogitsBinding> Plan(JsonObject snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

    var json = snapshot.ToJsonString();
        var planJson = Tokenizer.PlanLogitsProcessors(json);
        if (string.IsNullOrWhiteSpace(planJson))
        {
            return Array.Empty<LogitsBinding>();
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(planJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse logits plan emitted by native bridge.", ex);
        }

        if (root is not JsonArray array || array.Count == 0)
        {
            return Array.Empty<LogitsBinding>();
        }

        var bindings = new List<LogitsBinding>(array.Count);
        foreach (var element in array)
        {
            if (element is not JsonObject item)
            {
                continue;
            }

            if (!TryGetString(item, "category", out var category))
            {
                continue;
            }

            if (!TryGetString(item, "kind", out var kind))
            {
                continue;
            }

            if (!TryGetDouble(item, "value", out var value))
            {
                continue;
            }

            try
            {
                bindings.Add(new LogitsBinding(category, kind, value));
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return bindings.Count == 0 ? Array.Empty<LogitsBinding>() : bindings;
    }

    private static bool TryGetString(JsonObject container, string key, out string value)
    {
        value = string.Empty;
        if (container is null)
        {
            return false;
        }

        if (!container.TryGetPropertyValue(key, out var node) || node is null)
        {
            return false;
        }

        if (node is JsonValue jsonValue && jsonValue.TryGetValue(out string? text) && !string.IsNullOrWhiteSpace(text))
        {
            value = text;
            return true;
        }

        return false;
    }

    private static bool TryGetDouble(JsonObject container, string key, out double value)
    {
        value = 0d;
        if (container is null)
        {
            return false;
        }

        if (!container.TryGetPropertyValue(key, out var node) || node is null)
        {
            return false;
        }

        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue(out double direct))
            {
                value = direct;
                return true;
            }

            if (jsonValue.TryGetValue(out string? text) && double.TryParse(text, out var parsed))
            {
                value = parsed;
                return true;
            }
        }

        return false;
    }
}
