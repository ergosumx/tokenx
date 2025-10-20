namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// Provides configuration options for constructing a WordPiece model.
/// </summary>
public sealed record WordPieceModelOptions
{
    /// <summary>
    /// Gets an options instance with default values.
    /// </summary>
    public static WordPieceModelOptions Default { get; } = new();

    /// <summary>
    /// Gets the token used to represent unknown entries. Defaults to <c>"[UNK]"</c>.
    /// </summary>
    public string UnknownToken { get; init; } = "[UNK]";

    /// <summary>
    /// Gets the prefix inserted before continuing subwords.
    /// </summary>
    public string? ContinuingSubwordPrefix { get; init; }

    /// <summary>
    /// Gets the maximum number of characters allowed per word segment.
    /// </summary>
    public int? MaxInputCharsPerWord { get; init; }
}
