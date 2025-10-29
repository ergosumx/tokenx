namespace ErgoX.TokenX.SentencePiece.Options;

/// <summary>
/// Configuration options for sample encoding and scoring operations.
/// Controls the stochastic tokenization and scoring behavior for generating multiple alternative tokenizations.
/// </summary>
public sealed class SampleEncodeAndScoreOptions
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
    /// Gets or sets the number of stochastic samples to generate.
    /// Multiple samples provide alternative tokenizations with scores.
    /// Default is 1.
    /// </summary>
    public int NumSamples { get; set; } = 1;

    /// <summary>
    /// Gets or sets the smoothing parameter for sampling (temperature-like parameter).
    /// Higher values increase diversity; lower values favor more probable tokens.
    /// Default is 0.1.
    /// </summary>
    public float Alpha { get; set; } = 0.1f;

    /// <summary>
    /// Gets or sets a value indicating whether to sample without replacement.
    /// When true, each sample is unique; when false, samples can be duplicated.
    /// Default is false.
    /// </summary>
    public bool WithoutReplacement { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include the best (greedy) tokenization in the results.
    /// When true, the highest-scoring tokenization is always included.
    /// Default is true.
    /// </summary>
    public bool IncludeBest { get; set; } = true;
}

