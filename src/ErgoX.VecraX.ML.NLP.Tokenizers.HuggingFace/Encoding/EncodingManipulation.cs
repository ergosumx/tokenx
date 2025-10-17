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

        var totalLength = encodingsList.Sum(e => e.Length);
        var ids = new List<int>(totalLength);
        var tokens = new List<string>(totalLength);
        var offsets = new List<(int, int)>(totalLength);
        var typeIds = new List<uint>(totalLength);
        var attentionMask = new List<uint>(totalLength);
        var specialTokensMask = new List<uint>(totalLength);
        var wordIds = new List<int?>(totalLength);
        var sequenceIds = new List<int?>(totalLength);

        var currentOffset = 0;

        foreach (var encoding in encodingsList)
        {
            ids.AddRange(encoding.Ids);
            tokens.AddRange(encoding.Tokens);

            if (growingOffsets && currentOffset > 0)
            {
                // Adjust offsets to account for the growing text
                foreach (var (start, end) in encoding.Offsets)
                {
                    offsets.Add((start + currentOffset, end + currentOffset));
                }

                if (encoding.Offsets.Count > 0)
                {
                    var lastOffset = encoding.Offsets[encoding.Offsets.Count - 1];
                    currentOffset += lastOffset.End;
                }
            }
            else
            {
                offsets.AddRange(encoding.Offsets);
            }

            typeIds.AddRange(encoding.TypeIds);
            attentionMask.AddRange(encoding.AttentionMask);
            specialTokensMask.AddRange(encoding.SpecialTokensMask);
            wordIds.AddRange(encoding.WordIds);
            sequenceIds.AddRange(encoding.SequenceIds);
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
        var paddedIds = new List<int>(targetLength);
        var paddedTokens = new List<string>(targetLength);
        var paddedOffsets = new List<(int, int)>(targetLength);
        var paddedTypeIds = new List<uint>(targetLength);
        var paddedAttentionMask = new List<uint>(targetLength);
        var paddedSpecialTokensMask = new List<uint>(targetLength);
        var paddedWordIds = new List<int?>(targetLength);
        var paddedSequenceIds = new List<int?>(targetLength);

        if (direction == PaddingDirection.Left)
        {
            // Add padding at the beginning
            for (var i = 0; i < padLength; i++)
            {
                paddedIds.Add(padId);
                paddedTokens.Add(padToken);
                paddedOffsets.Add((0, 0));
                paddedTypeIds.Add(padTypeId);
                paddedAttentionMask.Add(0); // 0 for padding tokens
                paddedSpecialTokensMask.Add(1); // Padding tokens are treated as special
                paddedWordIds.Add(null);
                paddedSequenceIds.Add(null);
            }

            // Add original content
            paddedIds.AddRange(Ids);
            paddedTokens.AddRange(Tokens);
            paddedOffsets.AddRange(Offsets);
            paddedTypeIds.AddRange(TypeIds);
            paddedAttentionMask.AddRange(AttentionMask);
            paddedSpecialTokensMask.AddRange(SpecialTokensMask);
            paddedWordIds.AddRange(WordIds);
            paddedSequenceIds.AddRange(SequenceIds);
        }
        else // PaddingDirection.Right
        {
            // Add original content
            paddedIds.AddRange(Ids);
            paddedTokens.AddRange(Tokens);
            paddedOffsets.AddRange(Offsets);
            paddedTypeIds.AddRange(TypeIds);
            paddedAttentionMask.AddRange(AttentionMask);
            paddedSpecialTokensMask.AddRange(SpecialTokensMask);
            paddedWordIds.AddRange(WordIds);
            paddedSequenceIds.AddRange(SequenceIds);

            // Add padding at the end
            for (var i = 0; i < padLength; i++)
            {
                paddedIds.Add(padId);
                paddedTokens.Add(padToken);
                paddedOffsets.Add((0, 0));
                paddedTypeIds.Add(padTypeId);
                paddedAttentionMask.Add(0);
                paddedSpecialTokensMask.Add(1);
                paddedWordIds.Add(null);
                paddedSequenceIds.Add(null);
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
        return new EncodingResult(
            Ids.Skip(start).Take(length).ToList(),
            Tokens.Skip(start).Take(length).ToList(),
            Offsets.Skip(start).Take(length).ToList(),
            TypeIds.Skip(start).Take(length).ToList(),
            AttentionMask.Skip(start).Take(length).ToList(),
            SpecialTokensMask.Skip(start).Take(length).ToList(),
            WordIds.Skip(start).Take(length).ToList(),
            SequenceIds.Skip(start).Take(length).ToList(),
            Array.Empty<EncodingResult>());
    }
}
