namespace ErgoX.TokenX.HuggingFace.Generation;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.TokenX.HuggingFace;

/// <summary>
/// Computes stopping criteria plans by delegating to the native bridge and materializing managed representations.
/// </summary>
internal static class StoppingCriterionPlanner
{
    public static IReadOnlyList<StoppingCriterion> Plan(JsonObject snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        var json = snapshot.ToJsonString();
        var planJson = Tokenizer.PlanStoppingCriteria(json);
        if (string.IsNullOrWhiteSpace(planJson))
        {
            return Array.Empty<StoppingCriterion>();
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(planJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse stopping criteria plan emitted by native bridge.", ex);
        }

        if (root is not JsonArray array || array.Count == 0)
        {
            return Array.Empty<StoppingCriterion>();
        }

        var results = new List<StoppingCriterion>(array.Count);
        foreach (var element in array)
        {
            if (element is not JsonObject item)
            {
                continue;
            }

            if (!TryGetString(item, "kind", out var kind))
            {
                continue;
            }

            var value = TryGetInt(item, "value");
            var sequences = TryGetStringArray(item, "sequences");

            try
            {
                results.Add(new StoppingCriterion(kind, value, sequences));
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return results.Count == 0 ? Array.Empty<StoppingCriterion>() : results;
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

    private static int? TryGetInt(JsonObject container, string key)
    {
        if (container is null)
        {
            return null;
        }

        if (!container.TryGetPropertyValue(key, out var node) || node is null)
        {
            return null;
        }

        if (node is JsonValue jsonValue && TryConvertToInt(jsonValue, out var value))
        {
            return value;
        }

        return null;
    }

    private static bool TryConvertToInt(JsonValue value, out int result)
    {
        if (value.TryGetValue(out int direct))
        {
            result = direct;
            return true;
        }

        if (value.TryGetValue(out long longValue))
        {
            var clamped = Math.Clamp(longValue, int.MinValue, (long)int.MaxValue);
            result = (int)clamped;
            return true;
        }

        if (value.TryGetValue(out double doubleValue))
        {
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                result = default;
                return false;
            }

            var clamped = Math.Clamp(doubleValue, int.MinValue, int.MaxValue);
            result = (int)clamped;
            return true;
        }

        if (value.TryGetValue(out string? text) &&
            double.TryParse(text, out var parsed) &&
            !double.IsNaN(parsed) &&
            !double.IsInfinity(parsed))
        {
            var clamped = Math.Clamp(parsed, int.MinValue, int.MaxValue);
            result = (int)clamped;
            return true;
        }

        result = default;
        return false;
    }

    private static IReadOnlyList<string>? TryGetStringArray(JsonObject container, string key)
    {
        if (container is null)
        {
            return null;
        }

        if (!container.TryGetPropertyValue(key, out var node) || node is null)
        {
            return null;
        }

        return node switch
        {
            JsonArray array => ExtractStrings(array),
            JsonValue value when value.TryGetValue(out string? text) && !string.IsNullOrWhiteSpace(text)
                => new[] { text },
            _ => null
        };
    }

    private static IReadOnlyList<string>? ExtractStrings(JsonArray array)
    {
        var results = new List<string>(array.Count);
        foreach (var element in array)
        {
            if (element is JsonValue jsonValue && jsonValue.TryGetValue(out string? text) && !string.IsNullOrWhiteSpace(text))
            {
                results.Add(text);
            }
        }

        return results.Count == 0 ? null : results;
    }
}

