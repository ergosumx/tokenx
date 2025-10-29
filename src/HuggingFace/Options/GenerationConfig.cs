namespace ErgoX.TokenX.HuggingFace.Options;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.TokenX.HuggingFace.Generation;

/// <summary>
/// Represents the defaults defined in a Hugging Face generation_config.json file.
/// </summary>
public sealed class GenerationConfig
{
    private readonly JsonObject _root;

    private GenerationConfig(JsonObject root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    /// Parses a generation configuration from raw JSON.
    /// </summary>
    public static GenerationConfig FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Generation configuration JSON must be provided.", nameof(json));
        }

        var documentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var nodeOptions = new JsonNodeOptions
        {
            PropertyNameCaseInsensitive = false
        };

        var root = JsonNode.Parse(json, nodeOptions, documentOptions) as JsonObject
                   ?? throw new FormatException("The generation configuration root must be a JSON object.");

        return new GenerationConfig(root);
    }

    /// <summary>
    /// Creates a settings snapshot, applying optional overrides on top of the defaults.
    /// </summary>
    public GenerationSettings BuildSettings(GenerationOptions? overrides = null)
    {
        var snapshot = (JsonObject)_root.DeepClone();
        if (overrides is not null)
        {
            ApplyOverrides(snapshot, overrides);
        }

        var bindings = LogitsBindingPlanner.Plan(snapshot);
        var stoppingCriteria = StoppingCriterionPlanner.Plan(snapshot);
        return new GenerationSettings(snapshot, bindings, stoppingCriteria);
    }

    /// <summary>
    /// Produces a deep clone of the raw JSON object.
    /// </summary>
    public JsonObject ToJsonObject() => (JsonObject)_root.DeepClone();

    /// <summary>
    /// Serializes the configuration back to JSON text.
    /// </summary>
    public string ToJsonString(bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return _root.ToJsonString(options);
    }

    private static void ApplyOverrides(JsonObject target, GenerationOptions overrides)
    {
        ApplyNumberOverride(target, "temperature", overrides.TemperatureSpecified, overrides.Temperature);
        ApplyNumberOverride(target, "top_p", overrides.TopPSpecified, overrides.TopP);
        ApplyIntOverride(target, "top_k", overrides.TopKSpecified, overrides.TopK);
        ApplyNumberOverride(target, "repetition_penalty", overrides.RepetitionPenaltySpecified, overrides.RepetitionPenalty);
        ApplyIntOverride(target, "max_new_tokens", overrides.MaxNewTokensSpecified, overrides.MaxNewTokens);
        ApplyIntOverride(target, "min_new_tokens", overrides.MinNewTokensSpecified, overrides.MinNewTokens);
        ApplyBoolOverride(target, "do_sample", overrides.DoSampleSpecified, overrides.DoSample);
        ApplyIntOverride(target, "num_beams", overrides.NumBeamsSpecified, overrides.NumBeams);
        ApplyStopSequencesOverride(target, overrides);
        ApplyAdditionalParameters(target, overrides.AdditionalParameters);
    }

    private static void ApplyNumberOverride(JsonObject target, string key, bool specified, double? value)
    {
        if (!specified)
        {
            return;
        }

        SetNumber(target, key, value);
    }

    private static void ApplyIntOverride(JsonObject target, string key, bool specified, int? value)
    {
        if (!specified)
        {
            return;
        }

        SetInt(target, key, value);
    }

    private static void ApplyBoolOverride(JsonObject target, string key, bool specified, bool? value)
    {
        if (!specified)
        {
            return;
        }

        SetBool(target, key, value);
    }

    private static void ApplyStopSequencesOverride(JsonObject target, GenerationOptions overrides)
    {
        if (!overrides.StopSequencesSpecified)
        {
            return;
        }

        if (overrides.StopSequences is null)
        {
            RemoveProperty(target, "stop_sequences");
            RemoveProperty(target, "stop");
            return;
        }

        var array = new JsonArray();
        foreach (var entry in overrides.StopSequences)
        {
            array.Add(entry ?? string.Empty);
        }

        RemoveProperty(target, "stop");
        SetProperty(target, "stop_sequences", array);
    }

    private static void ApplyAdditionalParameters(JsonObject target, IDictionary<string, JsonNode?> parameters)
    {
        if (parameters.Count == 0)
        {
            return;
        }

        foreach (var pair in parameters.Where(p => !string.IsNullOrWhiteSpace(p.Key)))
        {
            var clone = pair.Value?.DeepClone();
            if (clone is null)
            {
                RemoveProperty(target, pair.Key);
            }
            else
            {
                SetProperty(target, pair.Key, clone);
            }
        }
    }

    private static void SetNumber(JsonObject target, string key, double? value)
    {
        if (value is null)
        {
            RemoveProperty(target, key);
            return;
        }

        SetProperty(target, key, JsonValue.Create(value.Value));
    }

    private static void SetInt(JsonObject target, string key, int? value)
    {
        if (value is null)
        {
            RemoveProperty(target, key);
            return;
        }

        SetProperty(target, key, JsonValue.Create(value.Value));
    }

    private static void SetBool(JsonObject target, string key, bool? value)
    {
        if (value is null)
        {
            RemoveProperty(target, key);
            return;
        }

        SetProperty(target, key, JsonValue.Create(value.Value));
    }

    private static void SetProperty(JsonObject target, string key, JsonNode? value)
    {
        var actual = FindExistingKey(target, key) ?? key;
        target[actual] = value;
    }

    private static void RemoveProperty(JsonObject target, string key)
    {
        var actual = FindExistingKey(target, key);
        if (actual is not null)
        {
            target.Remove(actual);
        }
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

