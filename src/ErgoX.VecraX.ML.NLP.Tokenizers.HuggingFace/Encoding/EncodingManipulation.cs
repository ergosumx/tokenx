using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// Encoding manipulation methods for merging, padding, and truncating encodings.
/// </summary>
public sealed partial class EncodingResult
{
    /// <summary>
    /// Merges multiple encodings into a single encoding.
    /// </summary>
    /// <param name="encodings">The encodings to merge.</param>
    /// <param name="growingOffsets">If true, offsets will be adjusted to account for the growing text.
    /// If false, offsets remain relative to their original text.</param>
    /// <returns>A new <see cref="EncodingResult"/> containing the merged encodings, or null if the list is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="encodings"/> is null.</exception>
    public static EncodingResult? Merge(IEnumerable<EncodingResult> encodings, bool growingOffsets = false)
    {
        if (encodings == null)
        {
            throw new ArgumentNullException(nameof(encodings));
        }

        var encodingsList = encodings.ToList();
        if (encodingsList.Count == 0)
        {
            return null;
        }

        if (encodingsList.Count == 1)
        {
            return encodingsList[0];
        }

        var totalLength = encodingsList.Sum(static e => e.Length);
        var ids = new int[totalLength];
        var tokens = new string[totalLength];
        var offsets = new (int, int)[totalLength];
        var typeIds = new uint[totalLength];
        var attentionMask = new uint[totalLength];
        var specialTokensMask = new uint[totalLength];
        var wordIds = new int?[totalLength];
        var sequenceIds = new int?[totalLength];

        var currentOffset = 0;
        var position = 0;

        foreach (var encoding in encodingsList)
        {
            var segmentLength = encoding.Length;
            for (var i = 0; i < segmentLength; i++)
            {
                var targetIndex = position + i;
                ids[targetIndex] = encoding.Ids[i];
                tokens[targetIndex] = encoding.Tokens[i];
                typeIds[targetIndex] = encoding.TypeIds[i];
                attentionMask[targetIndex] = encoding.AttentionMask[i];
                specialTokensMask[targetIndex] = encoding.SpecialTokensMask[i];
                wordIds[targetIndex] = encoding.WordIds[i];
                sequenceIds[targetIndex] = encoding.SequenceIds[i];

                var offset = encoding.Offsets[i];
                offsets[targetIndex] = growingOffsets && currentOffset > 0
                    ? (offset.Start + currentOffset, offset.End + currentOffset)
                    : offset;
            }

            if (growingOffsets && encoding.Offsets.Count > 0)
            {
                var lastOffset = encoding.Offsets[encoding.Offsets.Count - 1];
                currentOffset += lastOffset.End;
            }

            position += segmentLength;
        }

        return new EncodingResult(
            ids,
            tokens,
            offsets,
            typeIds,
            attentionMask,
            specialTokensMask,
            wordIds,
            sequenceIds,
            Array.Empty<EncodingResult>());
    }

    /// <summary>
    /// Pads the encoding to the specified target length.
    /// </summary>
    /// <param name="targetLength">The desired length after padding.</param>
    /// <param name="padId">The token ID to use for padding.</param>
    /// <param name="padTypeId">The type ID to use for padding tokens.</param>
    /// <param name="padToken">The token string to use for padding.</param>
    /// <param name="direction">The padding direction (Left or Right).</param>
    /// <returns>A new <see cref="EncodingResult"/> with padding applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="padToken"/> is null.</exception>
    public EncodingResult Pad(
        int targetLength,
        int padId,
        uint padTypeId,
        string padToken,
        PaddingDirection direction)
    {
        if (padToken == null)
        {
            throw new ArgumentNullException(nameof(padToken));
        }

        if (Length >= targetLength)
        {
            return this;
        }

        var padLength = targetLength - Length;
        var paddedIds = new int[targetLength];
        var paddedTokens = new string[targetLength];
        var paddedOffsets = new (int, int)[targetLength];
        var paddedTypeIds = new uint[targetLength];
        var paddedAttentionMask = new uint[targetLength];
        var paddedSpecialTokensMask = new uint[targetLength];
        var paddedWordIds = new int?[targetLength];
        var paddedSequenceIds = new int?[targetLength];

        if (direction == PaddingDirection.Left)
        {
            for (var i = 0; i < padLength; i++)
            {
                paddedIds[i] = padId;
                paddedTokens[i] = padToken;
                paddedOffsets[i] = (0, 0);
                paddedTypeIds[i] = padTypeId;
                paddedAttentionMask[i] = 0;
                paddedSpecialTokensMask[i] = 1;
                paddedWordIds[i] = null;
                paddedSequenceIds[i] = null;
            }

            for (var i = 0; i < Length; i++)
            {
                var targetIndex = i + padLength;
                paddedIds[targetIndex] = Ids[i];
                paddedTokens[targetIndex] = Tokens[i];
                paddedOffsets[targetIndex] = Offsets[i];
                paddedTypeIds[targetIndex] = TypeIds[i];
                paddedAttentionMask[targetIndex] = AttentionMask[i];
                paddedSpecialTokensMask[targetIndex] = SpecialTokensMask[i];
                paddedWordIds[targetIndex] = WordIds[i];
                paddedSequenceIds[targetIndex] = SequenceIds[i];
            }
        }
        else
        {
            for (var i = 0; i < Length; i++)
            {
                paddedIds[i] = Ids[i];
                paddedTokens[i] = Tokens[i];
                paddedOffsets[i] = Offsets[i];
                paddedTypeIds[i] = TypeIds[i];
                paddedAttentionMask[i] = AttentionMask[i];
                paddedSpecialTokensMask[i] = SpecialTokensMask[i];
                paddedWordIds[i] = WordIds[i];
                paddedSequenceIds[i] = SequenceIds[i];
            }

            var startIndex = Length;
            for (var i = 0; i < padLength; i++)
            {
                var targetIndex = startIndex + i;
                paddedIds[targetIndex] = padId;
                paddedTokens[targetIndex] = padToken;
                paddedOffsets[targetIndex] = (0, 0);
                paddedTypeIds[targetIndex] = padTypeId;
                paddedAttentionMask[targetIndex] = 0;
                paddedSpecialTokensMask[targetIndex] = 1;
                paddedWordIds[targetIndex] = null;
                paddedSequenceIds[targetIndex] = null;
            }
        }

        return new EncodingResult(
            paddedIds,
            paddedTokens,
            paddedOffsets,
            paddedTypeIds,
            paddedAttentionMask,
            paddedSpecialTokensMask,
            paddedWordIds,
            paddedSequenceIds,
            Overflowing);
    }

    /// <summary>
    /// Truncates the encoding to the specified maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum length after truncation.</param>
    /// <param name="stride">The stride for creating overflowing encodings.</param>
    /// <param name="direction">The truncation direction (Left or Right).</param>
    /// <returns>A new <see cref="EncodingResult"/> with truncation applied and overflowing parts stored.</returns>
    /// <exception cref="ArgumentException">Thrown when stride is greater than or equal to maxLength.</exception>
    public EncodingResult Truncate(
        int maxLength,
        int stride,
        TruncationDirection direction)
    {
        if (Length <= maxLength)
        {
            return this;
        }

        if (stride > 0 && stride >= maxLength)
        {
            throw new ArgumentException("Stride must be less than maxLength", nameof(stride));
        }

        if (maxLength == 0)
        {
            // Everything becomes overflowing - or return empty with no overflowing if stride=0
            if (stride == 0)
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
            else
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
                    new[] { this });
            }
        }

        // No overflowing if stride is 0
        if (stride == 0)
        {
            if (direction == TruncationDirection.Right)
            {
                var sliced = Slice(0, maxLength);
                return new EncodingResult(
                    sliced.Ids,
                    sliced.Tokens,
                    sliced.Offsets,
                    sliced.TypeIds,
                    sliced.AttentionMask,
                    sliced.SpecialTokensMask,
                    sliced.WordIds,
                    sliced.SequenceIds,
                    Array.Empty<EncodingResult>());
            }
            else
            {
                var sliced = Slice(Length - maxLength, maxLength);
                return new EncodingResult(
                    sliced.Ids,
                    sliced.Tokens,
                    sliced.Offsets,
                    sliced.TypeIds,
                    sliced.AttentionMask,
                    sliced.SpecialTokensMask,
                    sliced.WordIds,
                    sliced.SequenceIds,
                    Array.Empty<EncodingResult>());
            }
        }

        var overflowingList = new List<EncodingResult>();
        var offset = maxLength - stride;

        EncodingResult mainEncoding;

        if (direction == TruncationDirection.Right)
        {
            // Keep the left part, truncate right
            mainEncoding = Slice(0, maxLength);

            // Create overflowing chunks
            var position = offset;
            while (position < Length)
            {
                var chunkEnd = Math.Min(position + maxLength, Length);
                overflowingList.Add(Slice(position, chunkEnd - position));
                position += offset;
            }
        }
        else // TruncationDirection.Left
        {
            // Keep the right part, truncate left
            mainEncoding = Slice(Length - maxLength, maxLength);

            // Create overflowing chunks from left
            var position = Length - offset;
            while (position > 0)
            {
                var chunkStart = Math.Max(0, position - maxLength);
                overflowingList.Insert(0, Slice(chunkStart, position - chunkStart));
                position -= offset;
            }
        }

        return new EncodingResult(
            mainEncoding.Ids,
            mainEncoding.Tokens,
            mainEncoding.Offsets,
            mainEncoding.TypeIds,
            mainEncoding.AttentionMask,
            mainEncoding.SpecialTokensMask,
            mainEncoding.WordIds,
            mainEncoding.SequenceIds,
            overflowingList);
    }

    /// <summary>
    /// Slices the encoding to get a subset of tokens.
    /// </summary>
    /// <param name="start">The starting index (inclusive).</param>
    /// <param name="length">The number of tokens to include.</param>
    /// <returns>A new <see cref="EncodingResult"/> containing the specified slice.</returns>
    private EncodingResult Slice(int start, int length)
    {
        var ids = new int[length];
        var tokens = new string[length];
        var offsets = new (int, int)[length];
        var typeIds = new uint[length];
        var attentionMask = new uint[length];
        var specialTokensMask = new uint[length];
        var wordIds = new int?[length];
        var sequenceIds = new int?[length];

        for (var i = 0; i < length; i++)
        {
            var sourceIndex = start + i;
            ids[i] = Ids[sourceIndex];
            tokens[i] = Tokens[sourceIndex];
            offsets[i] = Offsets[sourceIndex];
            typeIds[i] = TypeIds[sourceIndex];
            attentionMask[i] = AttentionMask[sourceIndex];
            specialTokensMask[i] = SpecialTokensMask[sourceIndex];
            wordIds[i] = WordIds[sourceIndex];
            sequenceIds[i] = SequenceIds[sourceIndex];
        }

        return new EncodingResult(
            ids,
            tokens,
            offsets,
            typeIds,
            attentionMask,
            specialTokensMask,
            wordIds,
            sequenceIds,
            Array.Empty<EncodingResult>());
    }
}
