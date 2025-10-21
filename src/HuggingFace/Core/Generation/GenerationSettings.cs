namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Immutable view of the generation configuration that will be sent to the inference backend.
/// </summary>
public sealed class GenerationSettings
{
    private readonly JsonObject _values;
    private readonly IReadOnlyList<LogitsBinding> _logitsBindings;
    private readonly IReadOnlyList<StoppingCriterion> _stoppingCriteria;

    internal GenerationSettings(
        JsonObject values,
        IReadOnlyList<LogitsBinding> logitsBindings,
        IReadOnlyList<StoppingCriterion> stoppingCriteria)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        _logitsBindings = logitsBindings ?? throw new ArgumentNullException(nameof(logitsBindings));
        _stoppingCriteria = stoppingCriteria ?? throw new ArgumentNullException(nameof(stoppingCriteria));
    }

    /// <summary>
    /// Gets the configured sampling temperature.
    /// </summary>
    public double? Temperature => GetDouble(_values, "temperature");

    /// <summary>
    /// Gets the nucleus sampling probability.
    /// </summary>
    public double? TopP => GetDouble(_values, "top_p");

    /// <summary>
    /// Gets the top-k sampling value.
    /// </summary>
    public int? TopK => GetInt(_values, "top_k");

    /// <summary>
    /// Gets the repetition penalty.
    /// </summary>
    public double? RepetitionPenalty => GetDouble(_values, "repetition_penalty");

    /// <summary>
    /// Gets the maximum number of new tokens to generate.
    /// </summary>
    public int? MaxNewTokens => GetInt(_values, "max_new_tokens");

    /// <summary>
    /// Gets the minimum number of new tokens to generate.
    /// </summary>
    public int? MinNewTokens => GetInt(_values, "min_new_tokens");

    /// <summary>
    /// Gets whether sampling should be used.
    /// </summary>
    public bool? DoSample => GetBool(_values, "do_sample");

    /// <summary>
    /// Gets the number of beams configured for beam search.
    /// </summary>
    public int? NumBeams => GetInt(_values, "num_beams");

    /// <summary>
    /// Gets the configured stop sequences if present.
    /// </summary>
    public IReadOnlyList<string>? StopSequences => GetStringList(_values, "stop_sequences", "stop");

    /// <summary>
    /// Gets the derived logits processor and warper bindings for this configuration.
    /// </summary>
    public IReadOnlyList<LogitsBinding> LogitsBindings => _logitsBindings;

    /// <summary>
    /// Gets the derived stopping criteria.
    /// </summary>
    public IReadOnlyList<StoppingCriterion> StoppingCriteria => _stoppingCriteria;

    /// <summary>
    /// Gets whether streamed results should skip special tokens by default, if specified.
    /// </summary>
    public bool? SkipSpecialTokens => GetBool(_values, "skip_special_tokens");

    /// <summary>
    /// Returns a deep-cloned JSON object representing the settings.
    /// </summary>
    public JsonObject ToJsonObject() => (JsonObject)_values.DeepClone();

    /// <summary>
    /// Serializes the configuration to JSON text.
    /// </summary>
    public string ToJsonString(bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return _values.ToJsonString(options);
    }

    /// <summary>
    /// Attempts to retrieve the raw JSON node for the requested parameter.
    /// </summary>
    public bool TryGetRawParameter(string name, out JsonNode? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = null;
            return false;
        }

        var key = FindExistingKey(_values, name);
        if (key is null)
        {
            value = null;
            return false;
        }

        value = _values[key]?.DeepClone();
        return true;
    }

    private static double? GetDouble(JsonObject source, string key)
    {
        if (!TryGetNumericNode(source, key, out var node))
        {
            return null;
        }

        if (node is JsonValue value)
        {
            if (value.TryGetValue<double>(out var direct))
            {
                return direct;
            }

            if (value.TryGetValue<decimal>(out var decimalValue))
            {
                return (double)decimalValue;
            }

            if (value.TryGetValue<long>(out var longValue))
            {
                return Convert.ToDouble(longValue, CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<string>(out var text) && double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static int? GetInt(JsonObject source, string key)
    {
        if (!TryGetNumericNode(source, key, out var node))
        {
            return null;
        }

        if (node is JsonValue value)
        {
            if (value.TryGetValue<int>(out var direct))
            {
                return direct;
            }

            if (value.TryGetValue<long>(out var longValue))
            {
                return checked((int)longValue);
            }

            if (value.TryGetValue<double>(out var doubleValue))
            {
                return checked((int)doubleValue);
            }

            if (value.TryGetValue<string>(out var text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static bool? GetBool(JsonObject source, string key)
    {
        if (!TryGetProperty(source, key, out var node) || node is null)
        {
            return null;
        }

        if (node is JsonValue value)
        {
            if (value.TryGetValue<bool>(out var direct))
            {
                return direct;
            }

            if (value.TryGetValue<string>(out var text) && bool.TryParse(text, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static IReadOnlyList<string>? GetStringList(JsonObject source, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!TryGetProperty(source, key, out var node) || node is null)
            {
                continue;
            }

            var captured = ExtractStringList(node);
            if (captured is not null)
            {
                return captured;
            }
        }

        return null;
    }

    private static IReadOnlyList<string>? ExtractStringList(JsonNode node)
        => node switch
        {
            JsonArray array => ExtractStringListFromArray(array),
            JsonValue value => ExtractStringListFromValue(value),
            _ => null
        };

    private static IReadOnlyList<string>? ExtractStringListFromArray(JsonArray array)
    {
        if (array.Count == 0)
        {
            return null;
        }

        var buffer = new List<string>(array.Count);
        foreach (var item in array)
        {
            if (TryConvertToString(item, out var text))
            {
                buffer.Add(text);
            }
        }

        return buffer.Count == 0 ? null : buffer;
    }

    private static IReadOnlyList<string>? ExtractStringListFromValue(JsonValue value)
        => TryConvertToString(value, out var text) ? new[] { text } : null;

    private static bool TryConvertToString(JsonNode? node, out string result)
    {
        result = string.Empty;
        if (node is null)
        {
            return false;
        }

        if (node is JsonValue value)
        {
            if (value.TryGetValue<string>(out var text))
            {
                result = text;
                return true;
            }

            if (value.TryGetValue<long>(out var longValue))
            {
                result = longValue.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (value.TryGetValue<double>(out var doubleValue))
            {
                result = doubleValue.ToString(CultureInfo.InvariantCulture);
                return true;
            }
        }

        return false;
    }

    private static bool TryGetNumericNode(JsonObject source, string key, out JsonNode? node)
    {
        if (TryGetProperty(source, key, out node) && node is not null)
        {
            return true;
        }

        node = null;
        return false;
    }

    private static bool TryGetProperty(JsonObject source, string key, out JsonNode? node)
    {
        var actual = FindExistingKey(source, key);
        if (actual is null)
        {
            node = null;
            return false;
        }

        node = source[actual];
        return true;
    }

    private static string? FindExistingKey(JsonObject target, string key)
    {
        foreach (var (propertyName, _) in target)
        {
            if (string.Equals(propertyName, key, StringComparison.Ordinal))
            {
                return propertyName;
            }

            if (string.Equals(propertyName, key, StringComparison.OrdinalIgnoreCase))
            {
                return propertyName;
            }
        }

        return null;
    }
}
