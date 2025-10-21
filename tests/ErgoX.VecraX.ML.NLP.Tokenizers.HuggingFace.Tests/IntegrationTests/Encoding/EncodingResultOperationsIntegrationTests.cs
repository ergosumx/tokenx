namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class EncodingResultOperationsIntegrationTests : HuggingFaceTestBase, IDisposable
{
    private readonly Tokenizer tokenizer;

    public EncodingResultOperationsIntegrationTests()
    {
        tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    [Fact]
    public void Merge_combines_encodings_and_offsets()
    {
        var first = tokenizer.Encode("Quick brown fox");
        var second = tokenizer.Encode("jumps over the lazy dog");

        var merged = EncodingResult.Merge(new[] { first, second }, growingOffsets: false);
        Assert.NotNull(merged);
        Assert.Equal(first.Length + second.Length, merged!.Length);
        Assert.Equal(first.Offsets[first.Length - 1], merged.Offsets[first.Length - 1]);
        Assert.Equal(second.Offsets[0], merged.Offsets[first.Length]);

        var shifted = EncodingResult.Merge(new[] { first, second }, growingOffsets: true);
        Assert.NotNull(shifted);
        var shift = first.Offsets[^1].End;
        Assert.Equal(second.Offsets[0].Start + shift, shifted!.Offsets[first.Length].Start);
        Assert.Equal(second.Offsets[0].End + shift, shifted.Offsets[first.Length].End);
    }

    [Fact]
    public void Merge_validates_inputs()
    {
        Assert.Null(EncodingResult.Merge(Array.Empty<EncodingResult>(), growingOffsets: false));

        var encoding = tokenizer.Encode("sample");
        var inputs = new EncodingResult?[] { encoding, null };
        Assert.Throws<ArgumentException>(() => EncodingResult.Merge(inputs!, false));
    }

    [Fact]
    public void Pad_supports_left_and_right_padding()
    {
        var encoding = tokenizer.Encode("VecraX");
        var targetLength = encoding.Length + 3;

        var rightPad = encoding.Pad(targetLength, padId: 999, padTypeId: 7, padToken: "<pad>", PaddingDirection.Right);
        Assert.Equal(targetLength, rightPad.Length);
        Assert.Equal(999, rightPad.Ids[^1]);
        Assert.Equal("<pad>", rightPad.Tokens[^1]);
        Assert.Equal(0u, rightPad.AttentionMask[^1]);
        Assert.Equal(1u, rightPad.SpecialTokensMask[^1]);

        var leftPad = encoding.Pad(targetLength, padId: 111, padTypeId: 3, padToken: "<pad>", PaddingDirection.Left);
        Assert.Equal(targetLength, leftPad.Length);
        Assert.Equal(111, leftPad.Ids[0]);
        Assert.Equal("<pad>", leftPad.Tokens[0]);
        Assert.Equal(0u, leftPad.AttentionMask[0]);
        Assert.Equal(1u, leftPad.SpecialTokensMask[0]);

        Assert.Same(encoding, encoding.Pad(encoding.Length, 0, 0, "<pad>", PaddingDirection.Right));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.Pad(-1, 0, 0, "<pad>", PaddingDirection.Right));
        Assert.Throws<ArgumentNullException>(() => encoding.Pad(encoding.Length + 1, 0, 0, null!, PaddingDirection.Right));
    }

    [Fact]
    public void Truncate_generates_segments_and_overflow()
    {
        var longEncoding = tokenizer.Encode("This is a fairly long sentence designed to force truncation", "Second sequence content");
        Assert.True(longEncoding.Length > 10);

        var truncated = longEncoding.Truncate(8, stride: 2, TruncationDirection.Right);
        Assert.Equal(8, truncated.Length);
        Assert.NotEmpty(truncated.Overflowing);

        var left = longEncoding.Truncate(6, stride: 0, TruncationDirection.Left);
        Assert.Equal(6, left.Length);
        Assert.All(left.Ids, id => Assert.Contains(id, longEncoding.Ids));

        Assert.Same(longEncoding, longEncoding.Truncate(longEncoding.Length, stride: 0, TruncationDirection.Right));

        var empty = longEncoding.Truncate(0, stride: 0, TruncationDirection.Right);
        Assert.Equal(0, empty.Length);

        Assert.Throws<ArgumentOutOfRangeException>(() => longEncoding.Truncate(-1, 0, TruncationDirection.Right));
        Assert.Throws<ArgumentOutOfRangeException>(() => longEncoding.Truncate(1, -1, TruncationDirection.Right));
        Assert.Throws<ArgumentException>(() => longEncoding.Truncate(4, stride: 4, TruncationDirection.Right));
    }

    [Fact]
    public void WithPadding_helpers_extend_sequences()
    {
        var encoding = tokenizer.Encode("custom");
        var padded = encoding.WithPadding(encoding.Length + 2, 42, "<pad>");
        Assert.Equal(encoding.Length + 2, padded.Length);
        Assert.Equal(42, padded.Ids[^1]);
        Assert.Equal("<pad>", padded.Tokens[^1]);

        var left = encoding.WithLeftPadding(encoding.Length + 2, 17, "<pad>");
        Assert.Equal(encoding.Length + 2, left.Length);
        Assert.Equal(17, left.Ids[0]);
        Assert.Equal("<pad>", left.Tokens[0]);
    }

    [Fact]
    public void Word_to_token_and_char_mappings_resolve_indices()
    {
        var pair = tokenizer.Encode("Hello bright world", "And a second section");
        var tokenIndices = Enumerable.Range(0, pair.Length).Where(i => pair.WordIds[i].HasValue).ToArray();
        Assert.NotEmpty(tokenIndices);

        var firstToken = tokenIndices[0];
        var firstWord = pair.WordIds[firstToken] ?? throw new InvalidOperationException("Word id missing for paired encoding.");
        var firstSequence = pair.SequenceIds[firstToken] ?? 0;

        var range = pair.WordToTokens(firstWord, firstSequence);
        Assert.True(range.HasValue);
        var rangeValue = range.Value;
        for (var i = rangeValue.StartToken; i < rangeValue.EndToken; i++)
        {
            Assert.Equal(firstWord, pair.WordIds[i]);
        }

        var span = pair.WordToChars(firstWord, firstSequence);
        Assert.True(span.HasValue);
        var expectedStart = int.MaxValue;
        var expectedEnd = int.MinValue;
        for (var i = 0; i < pair.Length; i++)
        {
            if (pair.SequenceIds[i] == firstSequence && pair.WordIds[i] == firstWord)
            {
                expectedStart = Math.Min(expectedStart, pair.Offsets[i].Start);
                expectedEnd = Math.Max(expectedEnd, pair.Offsets[i].End);
            }
        }

        var spanValue = span.Value;
        Assert.Equal(expectedStart, spanValue.Start);
        Assert.Equal(expectedEnd, spanValue.End);

        Assert.Null(pair.WordToTokens(999, 0));
        Assert.Null(pair.WordToChars(999, 0));
    }

    [Fact]
    public void Token_and_char_queries_map_sequences_and_words()
    {
        using var pairTokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("bert-base-uncased"));
        var encoding = pairTokenizer.Encode("Testing mappings", "with another sequence");

    Assert.Contains(1, encoding.SequenceIds.Where(id => id.HasValue).Select(id => id.GetValueOrDefault()));

        var secondSequenceToken = Enumerable.Range(0, encoding.Length).First(i => encoding.SequenceIds[i] == 1);
        Assert.Equal(1, encoding.TokenToSequence(secondSequenceToken));
        Assert.Null(encoding.TokenToSequence(-1));

        var tokenChars = encoding.TokenToChars(secondSequenceToken);
        Assert.True(tokenChars.HasValue);
        Assert.Equal(1, tokenChars.Value.SequenceIndex);
        Assert.True(tokenChars.Value.Chars.End >= tokenChars.Value.Chars.Start);

        var padded = encoding.Pad(encoding.Length + 1, 77, 0, "<pad>", PaddingDirection.Right);
        var fallback = padded.TokenToChars(padded.Length - 1);
        Assert.True(fallback.HasValue);
        Assert.Equal(0, fallback.Value.SequenceIndex);

        var tokenWord = encoding.TokenToWord(secondSequenceToken);
        Assert.True(tokenWord.HasValue);
        Assert.Equal(1, tokenWord.Value.SequenceIndex);

        var charPosition = encoding.Offsets[secondSequenceToken].Start;
        var mappedToken = encoding.CharToToken(charPosition, 1);
        Assert.Equal(secondSequenceToken, mappedToken);
    var mappedWord = encoding.CharToWord(charPosition, 1);
    Assert.True(mappedWord.HasValue);
    var expectedWord = encoding.WordIds[secondSequenceToken] ?? throw new InvalidOperationException("Word id missing for mapped token.");
    Assert.Equal(expectedWord, mappedWord.Value);

        var padTokenIndex = padded.CharToToken(0, 0);
        Assert.True(padTokenIndex.HasValue);
        Assert.Equal(0, padTokenIndex.Value);
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.CharToToken(-1, 0));
    }

    public void Dispose()
    {
        tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
}
