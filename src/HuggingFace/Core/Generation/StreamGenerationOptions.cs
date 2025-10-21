namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Generation;

/// <summary>
/// Represents optional controls that influence streaming generation behavior.
/// </summary>
public sealed class StreamGenerationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether streamed tokens should exclude special tokens. When null, defaults are inferred from the generation configuration.
    /// </summary>
    public bool? SkipSpecialTokens { get; set; }
}
