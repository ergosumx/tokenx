namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

/// <summary>
/// Configuration options for chat template rendering.
/// </summary>
/// <remarks>
/// This class provides customization for how chat conversations are formatted into prompts
/// using the model's template. Includes control over generation prompt insertion and variable substitution.
/// </remarks>
public sealed class ChatTemplateOptions
{
    private readonly Dictionary<string, JsonNode?> additionalVariables = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets a value indicating whether to add a generation prompt token at the end of the formatted chat.
    /// When true, signals the model to start generating text after the chat history.
    /// Default is true.
    /// </summary>
    public bool AddGenerationPrompt { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional template override to use instead of the model's default chat template.
    /// When provided, this template is used for formatting instead of the one stored in the model configuration.
    /// Default is null (use model default template).
    /// </summary>
    public string? TemplateOverride { get; set; }

    /// <summary>
    /// Gets the dictionary of additional variables for template substitution.
    /// Variables in the template can reference these custom values during formatting.
    /// </summary>
    public IDictionary<string, JsonNode?> AdditionalVariables => additionalVariables;

    /// <summary>
    /// Sets or updates a template variable value.
    /// </summary>
    /// <param name="key">The variable name. Must not be null or whitespace.</param>
    /// <param name="value">The JSON value to associate with the variable. Can be null to remove a value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    /// <remarks>
    /// The value is deep-cloned if not null to avoid external mutations affecting the stored value.
    /// </remarks>
    public void SetVariable(string key, JsonNode? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Variable key must be provided.", nameof(key));
        }

        additionalVariables[key] = value?.DeepClone();
    }

    /// <summary>
    /// Removes a variable from the additional variables dictionary.
    /// </summary>
    /// <param name="key">The variable name to remove. Must not be null or whitespace.</param>
    /// <returns>True if the variable was found and removed; false if the key was null/empty or not found.</returns>
    public bool RemoveVariable(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return additionalVariables.Remove(key);
    }
}
