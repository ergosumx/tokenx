using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public enum TruncationStrategy
{
    LongestFirst,
    OnlyFirst,
    OnlySecond
}

public enum TruncationDirection
{
    Left,
    Right
}

public sealed class TruncationOptions
{
    public int MaxLength { get; }
    public int Stride { get; }
    public TruncationStrategy Strategy { get; }
    public TruncationDirection Direction { get; }

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
