using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

/// <summary>
/// Specifies the direction in which padding should be applied.
/// </summary>
public enum PaddingDirection
{
    /// <summary>
    /// Pad on the left side of the sequence.
    /// </summary>
    Left,

    /// <summary>
    /// Pad on the right side of the sequence (default for encoders).
    /// </summary>
    Right
}

/// <summary>
/// Configuration options for padding during tokenization.
/// </summary>
/// <remarks>
/// Padding ensures all sequences in a batch have the same length by adding pad tokens.
/// Right padding is typical for encoder-only models; left padding is common for decoder-only models.
/// </remarks>
public sealed class PaddingOptions
{
    /// <summary>
    /// Gets the direction in which padding is applied (left or right).
    /// </summary>
    public PaddingDirection Direction { get; }

    /// <summary>
    /// Gets the token ID used for padding.
    /// </summary>
    public uint PadId { get; }

    /// <summary>
    /// Gets the type ID (segment ID) assigned to pad tokens.
    /// </summary>
    public uint PadTypeId { get; }

    /// <summary>
    /// Gets the token string used for padding (e.g., "[PAD]").
    /// </summary>
    public string PadToken { get; }

    /// <summary>
    /// Gets the target sequence length. If specified, all sequences are padded to this length.
    /// If <c>null</c>, sequences are padded to the length of the longest sequence in the batch.
    /// </summary>
    public int? Length { get; }

    /// <summary>
    /// Gets the multiple to which sequence lengths should be padded.
    /// For example, if set to 8, lengths are padded to multiples of 8 (useful for GPU optimization).
    /// If <c>null</c> or less than 2, this setting is ignored.
    /// </summary>
    public int? PadToMultipleOf { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaddingOptions"/> class with specified padding settings.
    /// </summary>
    /// <param name="direction">The padding direction (left or right). Defaults to right.</param>
    /// <param name="padId">The token ID for padding tokens. Must be non-negative. Defaults to 0.</param>
    /// <param name="padTypeId">The type ID (segment ID) for padding tokens. Must be non-negative. Defaults to 0.</param>
    /// <param name="padToken">The token string for padding (e.g., "[PAD]"). Must not be null or empty.</param>
    /// <param name="length">The target sequence length. If <c>null</c>, padding is to the longest sequence in the batch.</param>
    /// <param name="padToMultipleOf">The multiple to which lengths should be padded. Ignored if less than 2. Defaults to <c>null</c>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="padId"/> or <paramref name="padTypeId"/> is negative.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="padToken"/> is null or empty.</exception>
    /// <example>
    /// <code>
    /// // Create padding options for right-padding to 512 tokens
    /// var options = new PaddingOptions(
    ///     direction: PaddingDirection.Right,
    ///     padId: 0,
    ///     padTypeId: 0,
    ///     padToken: "[PAD]",
    ///     length: 512
    /// );
    /// </code>
    /// </example>
    public PaddingOptions(
        PaddingDirection direction = PaddingDirection.Right,
        int padId = 0,
        int padTypeId = 0,
        string padToken = "[PAD]",
        int? length = null,
        int? padToMultipleOf = null)
    {
        if (padId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(padId), "Pad id cannot be negative.");
        }

        if (padTypeId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(padTypeId), "Pad type id cannot be negative.");
        }

        if (string.IsNullOrEmpty(padToken))
        {
            throw new ArgumentException("Pad token cannot be null or empty.", nameof(padToken));
        }

        if (padToMultipleOf is < 2)
        {
            padToMultipleOf = null;
        }

        Direction = direction;
    PadId = (uint)padId;
    PadTypeId = (uint)padTypeId;
        PadToken = padToken;
        Length = length;
        PadToMultipleOf = padToMultipleOf;
    }
}
