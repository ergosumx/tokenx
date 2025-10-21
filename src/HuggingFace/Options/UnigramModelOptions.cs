namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// Provides configuration options for constructing a SentencePiece-compatible Unigram model.
/// </summary>
public sealed record UnigramModelOptions
{
    /// <summary>
    /// Gets an options instance with default values.
    /// </summary>
    public static UnigramModelOptions Default { get; } = new();
}
