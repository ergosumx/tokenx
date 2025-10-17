using System;
using System.Linq;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// Position mapping methods for navigating between characters, words, tokens, and sequences.
/// </summary>
public sealed partial class EncodingResult
{
    /// <summary>
    /// Gets the token range for a specific word in a sequence.
    /// </summary>
    /// <param name="wordIndex">The word index to lookup.</param>
    /// <param name="sequenceIndex">The sequence index (0 for single sequences, 0 or 1 for pairs).</param>
    /// <returns>A tuple containing (startToken, endTokenExclusive) or null if the word is not found.</returns>
    public (int StartToken, int EndToken)? WordToTokens(int wordIndex, int sequenceIndex = 0)
    {
        int? start = null;
        int? end = null;

        for (var i = 0; i < WordIds.Count; i++)
        {
            // Check if this token belongs to the right sequence
            var seqId = SequenceIds[i];
            if (seqId.HasValue && seqId.Value != sequenceIndex)
            {
                continue;
            }

            // Check if this token belongs to the target word
            var wordId = WordIds[i];
            if (wordId == wordIndex)
            {
                if (!start.HasValue || i < start.Value)
                {
                    start = i;
                }

                if (!end.HasValue || i >= end.Value)
                {
                    end = i + 1;
                }
            }
        }

        return start.HasValue && end.HasValue
            ? (start.Value, end.Value)
            : null;
    }

    /// <summary>
    /// Gets the character offsets for a specific word in a sequence.
    /// </summary>
    /// <param name="wordIndex">The word index to lookup.</param>
    /// <param name="sequenceIndex">The sequence index (0 for single sequences, 0 or 1 for pairs).</param>
    /// <returns>A tuple containing (start, end) character positions or null if the word is not found.</returns>
    public (int Start, int End)? WordToChars(int wordIndex, int sequenceIndex = 0)
    {
        var tokenRange = WordToTokens(wordIndex, sequenceIndex);
        if (!tokenRange.HasValue)
        {
            return null;
        }

        var (startToken, endToken) = tokenRange.Value;
        if (endToken == 0 || startToken >= Offsets.Count)
        {
            return null;
        }

        var startOffset = Offsets[startToken];
        var endOffset = Offsets[endToken - 1];

        return (startOffset.Start, endOffset.End);
    }

    /// <summary>
    /// Gets the sequence index for a specific token.
    /// </summary>
    /// <param name="tokenIndex">The token index to lookup.</param>
    /// <returns>The sequence index or null if the token doesn't belong to any sequence.</returns>
    public int? TokenToSequence(int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= SequenceIds.Count)
        {
            return null;
        }

        return SequenceIds[tokenIndex];
    }

    /// <summary>
    /// Gets the character offsets for a specific token.
    /// </summary>
    /// <param name="tokenIndex">The token index to lookup.</param>
    /// <returns>A tuple containing (sequenceIndex, (start, end)) or null if the token is invalid.</returns>
    public (int SequenceIndex, (int Start, int End))? TokenToChars(int tokenIndex)
    {
        var seqIndex = TokenToSequence(tokenIndex);
        if (!seqIndex.HasValue || tokenIndex >= Offsets.Count)
        {
            return null;
        }

        var offset = Offsets[tokenIndex];
        return (seqIndex.Value, offset);
    }

    /// <summary>
    /// Gets the word index for a specific token.
    /// </summary>
    /// <param name="tokenIndex">The token index to lookup.</param>
    /// <returns>A tuple containing (sequenceIndex, wordIndex) or null if the token doesn't belong to a word.</returns>
    public (int SequenceIndex, int WordIndex)? TokenToWord(int tokenIndex)
    {
        var seqIndex = TokenToSequence(tokenIndex);
        if (!seqIndex.HasValue || tokenIndex >= WordIds.Count)
        {
            return null;
        }

        var wordId = WordIds[tokenIndex];
        if (!wordId.HasValue)
        {
            return null;
        }

        return (seqIndex.Value, wordId.Value);
    }

    /// <summary>
    /// Gets the token index that contains a specific character position.
    /// </summary>
    /// <param name="charPosition">The character position to lookup.</param>
    /// <param name="sequenceIndex">The sequence index (0 for single sequences, 0 or 1 for pairs).</param>
    /// <returns>The token index or null if no token contains the character.</returns>
    public int? CharToToken(int charPosition, int sequenceIndex = 0)
    {
        for (var i = 0; i < Offsets.Count; i++)
        {
            // Check if this token belongs to the right sequence
            var seqId = SequenceIds[i];
            if (seqId.HasValue && seqId.Value != sequenceIndex)
            {
                continue;
            }

            var (start, end) = Offsets[i];
            if (charPosition >= start && charPosition < end)
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the word index that contains a specific character position.
    /// </summary>
    /// <param name="charPosition">The character position to lookup.</param>
    /// <param name="sequenceIndex">The sequence index (0 for single sequences, 0 or 1 for pairs).</param>
    /// <returns>The word index or null if no word contains the character.</returns>
    public int? CharToWord(int charPosition, int sequenceIndex = 0)
    {
        var tokenIndex = CharToToken(charPosition, sequenceIndex);
        if (!tokenIndex.HasValue)
        {
            return null;
        }

        var wordInfo = TokenToWord(tokenIndex.Value);
        return wordInfo?.WordIndex;
    }
}
