using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public enum PaddingDirection
{
    Left,
    Right
}

public sealed class PaddingOptions
{
    public PaddingDirection Direction { get; }
    public uint PadId { get; }
    public uint PadTypeId { get; }
    public string PadToken { get; }
    public int? Length { get; }
    public int? PadToMultipleOf { get; }

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
