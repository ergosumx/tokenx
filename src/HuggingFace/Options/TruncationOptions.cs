using System;

namespace ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// Specifies the strategy for truncating text pairs when both exceed the maximum length.
/// </summary>
public enum TruncationStrategy
{
    /// <summary>
    /// Remove tokens from the longest sequence first until both sequences fit within max length.
    /// </summary>
    LongestFirst,

    /// <summary>
    /// Remove tokens only from the first sequence (or raise an error if it exceeds max length).
    /// </summary>
    OnlyFirst,

    /// <summary>
    /// Remove tokens only from the second sequence (if present).
    /// </summary>
    OnlySecond
}

/// <summary>
/// Specifies the direction in which truncation should be applied.
/// </summary>
public enum TruncationDirection
{
    /// <summary>
    /// Remove excess tokens from the left side of the sequence.
    /// </summary>
    Left,

    /// <summary>
    /// Remove excess tokens from the right side of the sequence (default).
    /// </summary>
    Right
}

/// <summary>
/// Configuration options for truncation during tokenization.
/// </summary>
/// <remarks>
/// Truncation ensures sequences do not exceed a specified maximum length.
/// When applied to text pairs, the truncation strategy determines which sequence is shortened.
/// For sliding window or stride-based processing, the Stride parameter enables overlapping truncations.
/// </remarks>
public sealed class TruncationOptions
{
    /// <summary>
    /// Gets the maximum sequence length. Sequences longer than this are truncated.
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// Gets the stride length for sliding window truncation.
    /// When processing long sequences, a stride creates overlapping windows.
    /// If 0 (default), no sliding window is used.
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// Gets the truncation strategy to apply when both texts in a pair exceed max length.
    /// </summary>
    public TruncationStrategy Strategy { get; }

    /// <summary>
    /// Gets the direction in which truncation is applied (left or right).
    /// </summary>
    public TruncationDirection Direction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncationOptions"/> class.
    /// </summary>
    /// <param name="maxLength">The maximum sequence length. Must be non-negative.</param>
    /// <param name="stride">The stride for sliding window truncation. Defaults to 0 (no sliding window). Must be non-negative.</param>
    /// <param name="strategy">The truncation strategy when handling text pairs. Defaults to LongestFirst.</param>
    /// <param name="direction">The direction in which tokens are removed. Defaults to Right.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> or <paramref name="stride"/> is negative, or when strategy/direction enums are invalid.</exception>
    /// <example>
    /// <code>
    /// // Truncate to 512 tokens, removing from the right side
    /// var options = new TruncationOptions(
    ///     maxLength: 512,
    ///     stride: 0,
    ///     strategy: TruncationStrategy.LongestFirst,
    ///     direction: TruncationDirection.Right
    /// );
    /// </code>
    /// </example>
    public TruncationOptions(
        int maxLength,
        int stride = 0,
        TruncationStrategy strategy = TruncationStrategy.LongestFirst,
        TruncationDirection direction = TruncationDirection.Right)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be negative.");
        }

        if (stride < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stride), "Stride cannot be negative.");
        }

        if (!Enum.IsDefined(typeof(TruncationStrategy), strategy))
        {
            throw new ArgumentOutOfRangeException(nameof(strategy), "Unknown truncation strategy specified.");
        }

        if (!Enum.IsDefined(typeof(TruncationDirection), direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), "Unknown truncation direction specified.");
        }

        MaxLength = maxLength;
        Stride = stride;
        Strategy = strategy;
        Direction = direction;
    }
}

