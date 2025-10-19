namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

public sealed class ChatTemplateOptions
{
    private readonly Dictionary<string, JsonNode?> additionalVariables = new(StringComparer.Ordinal);

    public bool AddGenerationPrompt { get; set; } = true;

    public string? TemplateOverride { get; set; }
    public IDictionary<string, JsonNode?> AdditionalVariables => additionalVariables;

    public void SetVariable(string key, JsonNode? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Variable key must be provided.", nameof(key));
        }

        additionalVariables[key] = value?.DeepClone();
    }

    public bool RemoveVariable(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return additionalVariables.Remove(key);
    }
}
