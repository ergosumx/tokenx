namespace ErgoX.TokenX.HuggingFace.Generation;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a planned stopping criterion derived from a generation configuration.
/// </summary>
public sealed class StoppingCriterion
{
    public StoppingCriterion(string kind, int? value, IReadOnlyList<string>? sequences)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException("Kind must be provided.", nameof(kind));
        }

        Kind = kind;
        Value = value;
        Sequences = sequences;
    }

    /// <summary>
    /// Gets the criterion kind (e.g. max_new_tokens or stop_sequences).
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Gets the numeric value associated with the criterion, when applicable.
    /// </summary>
    public int? Value { get; }

    /// <summary>
    /// Gets any stop sequences associated with the criterion.
    /// </summary>
    public IReadOnlyList<string>? Sequences { get; }

    /// <summary>
    /// Gets a value indicating whether the criterion enforces the maximum token budget.
    /// </summary>
    public bool IsMaxNewTokens => string.Equals(Kind, StoppingCriterionKinds.MaxNewTokens, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the criterion represents stop sequences.
    /// </summary>
    public bool IsStopSequences => string.Equals(Kind, StoppingCriterionKinds.StopSequences, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Provides strongly-typed constants for stopping criterion kinds.
/// </summary>
public static class StoppingCriterionKinds
{
    public const string MaxNewTokens = "max_new_tokens";
    public const string StopSequences = "stop_sequences";
}

