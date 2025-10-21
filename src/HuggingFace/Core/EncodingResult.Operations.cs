namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

public readonly record struct TokenRange(int StartToken, int EndToken);

public readonly record struct CharSpan(int Start, int End);

public sealed partial class EncodingResult
{
    public static EncodingResult? Merge(IEnumerable<EncodingResult> encodings, bool growingOffsets)
    {
        ArgumentNullException.ThrowIfNull(encodings);

        var materialized = encodings as IList<EncodingResult> ?? encodings.ToList();
        if (materialized.Count == 0)
        {
            return null;
        }

        var totalLength = 0;
        foreach (var encoding in materialized)
        {
            if (encoding is null)
            {
                throw new ArgumentException("Encodings collection cannot contain null entries.", nameof(encodings));
            }

            totalLength = checked(totalLength + encoding.Length);
        }

        var ids = new List<int>(totalLength);
        var tokens = new List<string>(totalLength);
        var offsets = new List<(int Start, int End)>(totalLength);
        var typeIds = new List<uint>(totalLength);
        var attentionMask = new List<uint>(totalLength);
        var specialTokensMask = new List<uint>(totalLength);
        var wordIds = new List<int?>(totalLength);
        var sequenceIds = new List<int?>(totalLength);
        var overflowing = new List<EncodingResult>();

        var offsetShift = 0;
        foreach (var encoding in materialized)
        {
            ids.AddRange(encoding.Ids);
            tokens.AddRange(encoding.Tokens);
            typeIds.AddRange(encoding.TypeIds);
            attentionMask.AddRange(encoding.AttentionMask);
            specialTokensMask.AddRange(encoding.SpecialTokensMask);
            wordIds.AddRange(encoding.WordIds);
            sequenceIds.AddRange(encoding.SequenceIds);

            if (encoding.Offsets.Count > 0)
            {
                if (growingOffsets)
                {
                    foreach (var (start, end) in encoding.Offsets)
                    {
                        offsets.Add((start + offsetShift, end + offsetShift));
                    }

                    offsetShift = offsets[^1].End;
                }
                else
                {
                    offsets.AddRange(encoding.Offsets);
                }
            }

            if (encoding.Overflowing.Count > 0)
            {
                overflowing.AddRange(encoding.Overflowing);
            }
        }

        return new EncodingResult(
            ids.ToArray(),
            tokens.ToArray(),
            offsets.ToArray(),
            typeIds.ToArray(),
            attentionMask.ToArray(),
            specialTokensMask.ToArray(),
            wordIds.ToArray(),
            sequenceIds.ToArray(),
            overflowing.Count == 0 ? Array.Empty<EncodingResult>() : overflowing.ToArray());
    }

    public EncodingResult Pad(int targetLength, int padId, int padTypeId, string padToken, PaddingDirection direction)
    {
        if (targetLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetLength));
        }

        ArgumentNullException.ThrowIfNull(padToken);

        if (targetLength <= Length)
        {
            return this;
        }

        var padCount = targetLength - Length;
        var newIds = new int[targetLength];
        var newTokens = new string[targetLength];
        var newOffsets = new (int Start, int End)[targetLength];
        var newTypeIds = new uint[targetLength];
        var newAttentionMask = new uint[targetLength];
        var newSpecialTokensMask = new uint[targetLength];
        var newWordIds = new int?[targetLength];
        var newSequenceIds = new int?[targetLength];

        var padTokenValue = padToken;
        var padTypeIdValue = (uint)Math.Max(0, padTypeId);

        if (direction == PaddingDirection.Left)
        {
            for (var i = 0; i < padCount; i++)
            {
                newIds[i] = padId;
                newTokens[i] = padTokenValue;
                newOffsets[i] = (0, 0);
                newTypeIds[i] = padTypeIdValue;
                newAttentionMask[i] = 0u;
                newSpecialTokensMask[i] = 1u;
                newWordIds[i] = null;
                newSequenceIds[i] = null;
            }

            for (var i = 0; i < Length; i++)
            {
                var targetIndex = padCount + i;
                newIds[targetIndex] = Ids[i];
                newTokens[targetIndex] = Tokens[i];
                newOffsets[targetIndex] = Offsets[i];
                newTypeIds[targetIndex] = i < TypeIds.Count ? TypeIds[i] : 0u;
                newAttentionMask[targetIndex] = i < AttentionMask.Count ? AttentionMask[i] : 1u;
                newSpecialTokensMask[targetIndex] = i < SpecialTokensMask.Count ? SpecialTokensMask[i] : 0u;
                newWordIds[targetIndex] = i < WordIds.Count ? WordIds[i] : null;
                newSequenceIds[targetIndex] = i < SequenceIds.Count ? SequenceIds[i] : null;
            }
        }
        else
        {
            for (var i = 0; i < Length; i++)
            {
                newIds[i] = Ids[i];
                newTokens[i] = Tokens[i];
                newOffsets[i] = Offsets[i];
                newTypeIds[i] = i < TypeIds.Count ? TypeIds[i] : 0u;
                newAttentionMask[i] = i < AttentionMask.Count ? AttentionMask[i] : 1u;
                newSpecialTokensMask[i] = i < SpecialTokensMask.Count ? SpecialTokensMask[i] : 0u;
                newWordIds[i] = i < WordIds.Count ? WordIds[i] : null;
                newSequenceIds[i] = i < SequenceIds.Count ? SequenceIds[i] : null;
            }

            for (var i = 0; i < padCount; i++)
            {
                var targetIndex = Length + i;
                newIds[targetIndex] = padId;
                newTokens[targetIndex] = padTokenValue;
                newOffsets[targetIndex] = (0, 0);
                newTypeIds[targetIndex] = padTypeIdValue;
                newAttentionMask[targetIndex] = 0u;
                newSpecialTokensMask[targetIndex] = 1u;
                newWordIds[targetIndex] = null;
                newSequenceIds[targetIndex] = null;
            }
        }

        return new EncodingResult(
            newIds,
            newTokens,
            newOffsets,
            newTypeIds,
            newAttentionMask,
            newSpecialTokensMask,
            newWordIds,
            newSequenceIds,
            Overflowing);
    }

    public EncodingResult Truncate(int maxLength, int stride, TruncationDirection direction)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        if (stride < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stride));
        }

        if (maxLength == 0)
        {
            return new EncodingResult(
                Array.Empty<int>(),
                Array.Empty<string>(),
                Array.Empty<(int, int)>(),
                Array.Empty<uint>(),
                Array.Empty<uint>(),
                Array.Empty<uint>(),
                Array.Empty<int?>(),
                Array.Empty<int?>(),
                Array.Empty<EncodingResult>());
        }

        if (maxLength >= Length)
        {
            return this;
        }

        if (stride >= maxLength)
        {
            throw new ArgumentException("Stride must be less than the maximum length.", nameof(stride));
        }

        if (direction == TruncationDirection.Left)
        {
            var start = Length - maxLength;
            return CreateSegment(this, start, maxLength, Array.Empty<EncodingResult>());
        }

        var overflowSegments = new List<EncodingResult>();
        if (stride > 0)
        {
            var step = maxLength - stride;
            var nextStart = maxLength - stride;
            if (nextStart <= 0)
            {
                nextStart = maxLength;
            }

            while (nextStart < Length)
            {
                var segmentLength = Math.Min(maxLength, Length - nextStart);
                overflowSegments.Add(CreateSegment(this, nextStart, segmentLength, Array.Empty<EncodingResult>()));
                if (segmentLength + nextStart >= Length)
                {
                    break;
                }

                nextStart += step;
            }
        }

        return CreateSegment(
            this,
            0,
            maxLength,
            overflowSegments.Count == 0 ? Array.Empty<EncodingResult>() : overflowSegments);
    }

    public TokenRange? WordToTokens(int wordIndex, int sequenceIndex)
    {
        if (wordIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        }

        if (sequenceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
        }

        var startToken = -1;
        var endToken = -1;

        for (var i = 0; i < Length; i++)
        {
            if (!MatchesSequence(SequenceIds[i], sequenceIndex))
            {
                continue;
            }

            if (WordIds[i].HasValue && WordIds[i]!.Value == wordIndex)
            {
                if (startToken == -1)
                {
                    startToken = i;
                }

                endToken = i;
            }
        }

        return startToken == -1 ? null : new TokenRange(startToken, endToken + 1);
    }

    public CharSpan? WordToChars(int wordIndex, int sequenceIndex)
    {
        if (wordIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        }

        if (sequenceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
        }

        var hasMatch = false;
        var start = int.MaxValue;
        var end = int.MinValue;

        for (var i = 0; i < Length; i++)
        {
            if (!MatchesSequence(SequenceIds[i], sequenceIndex))
            {
                continue;
            }

            if (WordIds[i].HasValue && WordIds[i]!.Value == wordIndex)
            {
                var (tokenStart, tokenEnd) = Offsets[i];
                if (tokenStart < start)
                {
                    start = tokenStart;
                }

                if (tokenEnd > end)
                {
                    end = tokenEnd;
                }

                hasMatch = true;
            }
        }

        return hasMatch ? new CharSpan(start, end) : null;
    }

    public int? TokenToSequence(int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= Length)
        {
            return null;
        }

        return SequenceIds[tokenIndex];
    }

    public (int SequenceIndex, CharSpan Chars)? TokenToChars(int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= Length)
        {
            return null;
        }

        var sequence = SequenceIds[tokenIndex];
        if (sequence.HasValue)
        {
            var (start, end) = Offsets[tokenIndex];
            return (sequence.Value, new CharSpan(start, end));
        }

        if (sequence is null && MatchesSequence(null, 0))
        {
            var (start, end) = Offsets[tokenIndex];
            return (0, new CharSpan(start, end));
        }

        return null;
    }

    public (int SequenceIndex, int? WordIndex)? TokenToWord(int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= Length)
        {
            return null;
        }

        var sequence = SequenceIds[tokenIndex];
        var sequenceValue = sequence ?? 0;
        return (sequenceValue, WordIds[tokenIndex]);
    }

    public int? CharToToken(int charPosition, int sequenceIndex)
    {
        if (charPosition < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(charPosition));
        }

        if (sequenceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
        }

        for (var i = 0; i < Length; i++)
        {
            if (!MatchesSequence(SequenceIds[i], sequenceIndex))
            {
                continue;
            }

            var (start, end) = Offsets[i];
            if (start == end)
            {
                if (charPosition == start)
                {
                    return i;
                }

                continue;
            }

            if (start <= charPosition && charPosition < end)
            {
                return i;
            }
        }

        return null;
    }

    public int? CharToWord(int charPosition, int sequenceIndex)
    {
        var tokenIndex = CharToToken(charPosition, sequenceIndex);
        if (!tokenIndex.HasValue)
        {
            return null;
        }

        var word = WordIds[tokenIndex.Value];
        return word;
    }

    private static EncodingResult CreateSegment(
        EncodingResult source,
        int start,
        int length,
        IReadOnlyList<EncodingResult> overflowing)
    {
        if (start < 0 || length < 0 || start > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (start + length > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (length == 0)
        {
            return new EncodingResult(
                Array.Empty<int>(),
                Array.Empty<string>(),
                Array.Empty<(int, int)>(),
                Array.Empty<uint>(),
                Array.Empty<uint>(),
                Array.Empty<uint>(),
                Array.Empty<int?>(),
                Array.Empty<int?>(),
                overflowing);
        }

        var ids = Slice(source.Ids, start, length);
        var tokens = Slice(source.Tokens, start, length);
        var offsets = Slice(source.Offsets, start, length);
        var typeIds = Slice(source.TypeIds, start, length);
        var attentionMask = Slice(source.AttentionMask, start, length);
        var specialTokensMask = Slice(source.SpecialTokensMask, start, length);
        var wordIds = Slice(source.WordIds, start, length);
        var sequenceIds = Slice(source.SequenceIds, start, length);

        return new EncodingResult(
            ids,
            tokens,
            offsets,
            typeIds,
            attentionMask,
            specialTokensMask,
            wordIds,
            sequenceIds,
            overflowing);
    }

    private static T[] Slice<T>(IReadOnlyList<T> source, int start, int length)
    {
        var result = new T[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = source[start + i];
        }

        return result;
    }

    private static bool MatchesSequence(int? sequenceId, int sequenceIndex)
        => sequenceId.HasValue ? sequenceId.Value == sequenceIndex : sequenceIndex == 0;
}
