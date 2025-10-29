namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Linq;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class EncodingEdgeCaseIntegrationTests : HuggingFaceTestBase, IDisposable
{
    private readonly Tokenizer _tokenizer;

    public EncodingEdgeCaseIntegrationTests()
    {
        _tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    [Fact]
    public void Encode_EmptyString_ReturnsValidEncoding()
    {
        var encoding = _tokenizer.Encode("");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length >= 0);
    }

    [Fact]
    public void Encode_SingleCharacter_ReturnsValidEncoding()
    {
        var encoding = _tokenizer.Encode("a");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
        Assert.NotEmpty(encoding.Tokens);
    }

    [Fact]
    public void Encode_SpecialCharacters_HandlesCorrectly()
    {
        var encoding = _tokenizer.Encode("@#$%^&*()");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Encode_Unicode_HandlesCorrectly()
    {
        var encoding = _tokenizer.Encode("Hello 世界 🌍");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Encode_WithPair_BothSequencesPresent()
    {
        var encoding = _tokenizer.Encode("First sequence", "Second sequence");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);

        var hasSequence0 = encoding.SequenceIds.Any(id => id == 0);
        var hasSequence1 = encoding.SequenceIds.Any(id => id == 1);

        Assert.True(hasSequence0 || hasSequence1);
    }

    [Fact]
    public void Pad_WithZeroLength_ThrowsArgumentOutOfRangeException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.Pad(-5, 0, 0, "<pad>", PaddingDirection.Right));
    }

    [Fact]
    public void Pad_WithNullPadToken_ThrowsArgumentNullException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentNullException>(() => encoding.Pad(encoding.Length + 1, 0, 0, null!, PaddingDirection.Right));
    }

    [Fact]
    public void Truncate_WithZeroLength_ReturnsEmptyEncoding()
    {
        var encoding = _tokenizer.Encode("This is a test");
        var truncated = encoding.Truncate(0, 0, TruncationDirection.Right);

        Assert.NotNull(truncated);
        Assert.Equal(0, truncated.Length);
    }

    [Fact]
    public void Truncate_WithNegativeLength_ThrowsArgumentOutOfRangeException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.Truncate(-1, 0, TruncationDirection.Right));
    }

    [Fact]
    public void Truncate_WithNegativeStride_ThrowsArgumentOutOfRangeException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.Truncate(2, -1, TruncationDirection.Right));
    }

    [Fact]
    public void TokenToSequence_WithInvalidIndex_ReturnsNull()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Null(encoding.TokenToSequence(-1));
        Assert.Null(encoding.TokenToSequence(encoding.Length + 100));
    }

    [Fact]
    public void TokenToWord_WithInvalidIndex_ReturnsNull()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Null(encoding.TokenToWord(-1));
        Assert.Null(encoding.TokenToWord(encoding.Length + 100));
    }

    [Fact]
    public void CharToToken_WithNegativeChar_ThrowsArgumentOutOfRangeException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.CharToToken(-1, 0));
    }

    [Fact]
    public void CharToToken_WithNegativeSequence_ThrowsArgumentOutOfRangeException()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => encoding.CharToToken(0, -1));
    }

    [Fact]
    public void Merge_WithNullEncodings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EncodingResult.Merge(null!, false));
    }

    [Fact]
    public void Merge_WithEmptyArray_ReturnsNull()
    {
        var result = EncodingResult.Merge(Array.Empty<EncodingResult>(), false);
        Assert.Null(result);
    }

    [Fact]
    public void Merge_WithNullElement_ThrowsArgumentException()
    {
        var encoding1 = _tokenizer.Encode("test");
        var encodings = new EncodingResult?[] { encoding1, null };

        Assert.Throws<ArgumentException>(() => EncodingResult.Merge(encodings!, false));
    }

    [Fact]
    public void Merge_WithGrowingOffsets_ShiftsOffsets()
    {
        var encoding1 = _tokenizer.Encode("Hello");
        var encoding2 = _tokenizer.Encode("World");

        var merged = EncodingResult.Merge(new[] { encoding1, encoding2 }, true);

        Assert.NotNull(merged);
        Assert.True(merged!.Length >= encoding1.Length + encoding2.Length);
    }

    [Fact]
    public void WordToTokens_WithInvalidWordId_ReturnsNull()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Null(encoding.WordToTokens(99999, 0));
    }

    [Fact]
    public void WordToChars_WithInvalidWordId_ReturnsNull()
    {
        var encoding = _tokenizer.Encode("test");
        Assert.Null(encoding.WordToChars(99999, 0));
    }

    [Fact]
    public void EncodingResult_PropertiesNotNull()
    {
        var encoding = _tokenizer.Encode("test");

        Assert.NotNull(encoding.Ids);
        Assert.NotNull(encoding.TypeIds);
        Assert.NotNull(encoding.Tokens);
        Assert.NotNull(encoding.Offsets);
        Assert.NotNull(encoding.SpecialTokensMask);
        Assert.NotNull(encoding.AttentionMask);
        Assert.NotNull(encoding.WordIds);
        Assert.NotNull(encoding.SequenceIds);
    }

    [Fact]
    public void EncodingResult_LengthConsistent()
    {
        var encoding = _tokenizer.Encode("test text");

        Assert.Equal(encoding.Ids.Count, encoding.Length);
        Assert.Equal(encoding.TypeIds.Count, encoding.Length);
        Assert.Equal(encoding.Tokens.Count, encoding.Length);
        Assert.Equal(encoding.Offsets.Count, encoding.Length);
        Assert.Equal(encoding.SpecialTokensMask.Count, encoding.Length);
        Assert.Equal(encoding.AttentionMask.Count, encoding.Length);
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
}

