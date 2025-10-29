namespace ErgoX.TokenX.SentencePiece.Options;

/// <summary>
/// Configuration options for SentencePiece encoding operations.
/// Controls tokenization behavior including special token insertion, sampling, and N-best alternatives.
/// </summary>
public sealed class EncodeOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to add a beginning-of-sentence (BOS) token.
    /// Default is false.
    /// </summary>
    public bool AddBos { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add an end-of-sentence (EOS) token.
    /// Default is false.
    /// </summary>
    public bool AddEos { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to reverse the token sequence.
    /// Default is false.
    /// </summary>
    public bool Reverse { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to emit unknown (out-of-vocabulary) pieces in the output.
    /// Default is false.
    /// </summary>
    public bool EmitUnknownPiece { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable sampling-based encoding instead of deterministic greedy encoding.
    /// When enabled with a positive <see cref="NBestSize"/> and <see cref="Alpha"/>, provides stochastic tokenization.
    /// Default is false.
    /// </summary>
    public bool EnableSampling { get; set; }

    /// <summary>
    /// Gets or sets the size of the N-best candidates to consider during encoding.
    /// Positive values enable N-best encoding; -1 disables it (greedy encoding only).
    /// Default is -1 (disabled).
    /// </summary>
    public int NBestSize { get; set; } = -1;

    /// <summary>
    /// Gets or sets the smoothing parameter for sampling-based encoding (temperature-like parameter).
    /// Higher values increase randomness; lower values favor more probable tokens.
    /// Only used when <see cref="EnableSampling"/> is true.
    /// Default is 0.0 (deterministic).
    /// </summary>
    public float Alpha { get; set; }
}

