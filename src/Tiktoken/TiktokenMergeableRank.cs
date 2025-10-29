namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;

using System;

/// <summary>
/// Represents a mergeable token entry in a TikToken vocabulary.
/// </summary>
public readonly struct TiktokenMergeableRank
{
    public TiktokenMergeableRank(ReadOnlyMemory<byte> token, int rank)
    {
        if (rank < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be non-negative.");
        }

        Token = token.ToArray();
        Rank = rank;
    }

    public ReadOnlyMemory<byte> Token { get; }

    public int Rank { get; }
}
