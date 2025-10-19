namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;

using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Chat;

/// <summary>
/// Represents a streaming generation request that preserves the resolved settings and derived plans.
/// </summary>
public sealed class StreamingGenerationRequest
{
    public StreamingGenerationRequest(
        string prompt,
        GenerationSettings settings,
        IReadOnlyList<ChatMessage>? messages,
        bool skipSpecialTokens)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("A prompt must be provided for streaming generation.", nameof(prompt));
        }

        Prompt = prompt;
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Messages = messages;
        SkipSpecialTokens = skipSpecialTokens;
    }

    /// <summary>
    /// Gets the prompt text that should be supplied to an inference backend.
    /// </summary>
    public string Prompt { get; }

    /// <summary>
    /// Gets the resolved generation settings.
    /// </summary>
    public GenerationSettings Settings { get; }

    /// <summary>
    /// Gets the original chat messages when the request was built from conversational history.
    /// </summary>
    public IReadOnlyList<ChatMessage>? Messages { get; }

    /// <summary>
    /// Gets the derived logits warpers and processors.
    /// </summary>
    public IReadOnlyList<LogitsBinding> LogitsBindings => Settings.LogitsBindings;

    /// <summary>
    /// Gets the stopping criteria associated with the request.
    /// </summary>
    public IReadOnlyList<StoppingCriterion> StoppingCriteria => Settings.StoppingCriteria;

    /// <summary>
    /// Gets a value indicating whether streamed tokens should omit special tokens by default.
    /// </summary>
    public bool SkipSpecialTokens { get; }
}
