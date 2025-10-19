namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

/// <summary>
/// Represents a fully materialized generation invocation containing the prompt and resolved settings.
/// </summary>
public sealed class GenerationRequest
{
    public GenerationRequest(string prompt, GenerationSettings settings, IReadOnlyList<ChatMessage>? messages)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("A prompt must be provided for generation.", nameof(prompt));
        }

        Prompt = prompt;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Messages = messages;
    }

    /// <summary>
    /// Gets the prompt text that should be sent to the inference backend.
    /// </summary>
    public string Prompt { get; }

    /// <summary>
    /// Gets the resolved generation settings.
    /// </summary>
    public GenerationSettings Settings { get; }

    /// <summary>
    /// Gets the original chat messages if the request was built from structured history.
    /// </summary>
    public IReadOnlyList<ChatMessage>? Messages { get; }
}
