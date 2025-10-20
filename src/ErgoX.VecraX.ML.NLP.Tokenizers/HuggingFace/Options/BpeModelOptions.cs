namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// Provides configuration options for constructing a Byte-Pair Encoding (BPE) model.
/// </summary>
public sealed record BpeModelOptions
{
    /// <summary>
    /// Gets an options instance with default values.
    /// </summary>
    public static BpeModelOptions Default { get; } = new();

    /// <summary>
    /// Gets the dropout value applied during sampling (set to <c>null</c> to disable).
    /// </summary>
    public float? Dropout { get; init; }

    /// <summary>
    /// Gets the token used to represent unknown entries.
    /// </summary>
    public string? UnknownToken { get; init; }

    /// <summary>
    /// Gets the prefix inserted before continuing subwords.
    /// </summary>
    public string? ContinuingSubwordPrefix { get; init; }

    /// <summary>
    /// Gets the suffix appended when marking the end of a word.
    /// </summary>
    public string? EndOfWordSuffix { get; init; }

    /// <summary>
    /// Gets a value indicating whether unknown tokens should be fused during merges.
    /// </summary>
    public bool FuseUnknownTokens { get; init; }

    /// <summary>
    /// Gets a value indicating whether byte fallback should be enabled.
    /// </summary>
    public bool EnableByteFallback { get; init; }
}
