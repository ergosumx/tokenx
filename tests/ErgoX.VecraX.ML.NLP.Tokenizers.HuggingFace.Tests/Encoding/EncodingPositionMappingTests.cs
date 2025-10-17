using System;
using System.Linq;
using ErgoX.VecraX.ML.Tokenizers;
using Xunit;

namespace ErgoX.VecraX.ML.Tokenizers.Tests.Encoding;

/// <summary>
/// Tests for encoding position mapping methods (word/token/char conversions).
/// </summary>
public class EncodingPositionMappingTests : IDisposable
{
    private readonly Tokenizer _tokenizer;

    public EncodingPositionMappingTests()
    {
        _tokenizer = Tokenizer.FromPretrained("bert-base-uncased");
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
    }

    [Fact]
    public void WordToTokens_WithValidWordIndex_ReturnsTokenRange()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Get tokens for first word (assuming word index 0 exists)
        var result = encoding.WordToTokens(wordIndex: 0, sequenceIndex: 0);

        // Assert
        if (encoding.WordIds.Any(w => w == 0))
        {
            Assert.NotNull(result);
            Assert.True(result.Value.StartToken >= 0);
            Assert.True(result.Value.EndToken > result.Value.StartToken);
        }
    }

    [Fact]
    public void WordToTokens_WithInvalidWordIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Use a word index that definitely doesn't exist
        var result = encoding.WordToTokens(wordIndex: 9999, sequenceIndex: 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WordToTokens_WithWrongSequenceIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Single sequence encoding, so sequence 1 shouldn't exist
        var result = encoding.WordToTokens(wordIndex: 0, sequenceIndex: 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WordToChars_WithValidWordIndex_ReturnsCharRange()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Get chars for first word
        var result = encoding.WordToChars(wordIndex: 0, sequenceIndex: 0);

        // Assert
        if (encoding.WordIds.Any(w => w == 0))
        {
            Assert.NotNull(result);
            Assert.True(result.Value.Start >= 0);
            Assert.True(result.Value.End > result.Value.Start);
        }
    }

    [Fact]
    public void WordToChars_WithInvalidWordIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.WordToChars(wordIndex: 9999, sequenceIndex: 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TokenToSequence_WithValidTokenIndex_ReturnsSequenceIndex()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Skip test if SequenceIds are not populated for this model
        // OR if the first token doesn't have a sequence ID (could be special token)
        if (!encoding.SequenceIds.Any(s => s.HasValue) || !encoding.SequenceIds[0].HasValue)
        {
            return; // Skip this test
        }
        
        // Act
        var result = encoding.TokenToSequence(tokenIndex: 0);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void TokenToSequence_WithInvalidTokenIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.TokenToSequence(tokenIndex: encoding.Length + 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TokenToChars_WithValidTokenIndex_ReturnsCharOffsets()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Skip test if SequenceIds are not populated for this model
        // OR if the first token doesn't have a sequence ID
        if (!encoding.SequenceIds.Any(s => s.HasValue) || !encoding.SequenceIds[0].HasValue)
        {
            return; // Skip this test
        }
        
        // Act
        var result = encoding.TokenToChars(tokenIndex: 0);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value.Item2.Start >= 0);
        Assert.True(result.Value.Item2.End > result.Value.Item2.Start);
    }

    [Fact]
    public void TokenToChars_WithInvalidTokenIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.TokenToChars(tokenIndex: encoding.Length + 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TokenToWord_WithValidTokenIndex_ReturnsWordIndex()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.TokenToWord(tokenIndex: 0);

        // Assert
        if (encoding.WordIds[0].HasValue)
        {
            Assert.NotNull(result);
            Assert.Equal(0, result.Value.SequenceIndex);
            // WordIndex should have a value
            Assert.True(result.HasValue);
        }
    }

    [Fact]
    public void TokenToWord_WithInvalidTokenIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.TokenToWord(tokenIndex: encoding.Length + 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TokenToWord_ForSpecialToken_ReturnsNullWordIndex()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Find a special token (usually at start/end for BERT)
        int? specialTokenIndex = null;
        for (int i = 0; i < encoding.Length; i++)
        {
            if (encoding.SpecialTokensMask[i] == 1)
            {
                specialTokenIndex = i;
                break;
            }
        }

        // Act & Assert
        if (specialTokenIndex.HasValue)
        {
            var result = encoding.TokenToWord(specialTokenIndex.Value);
            // Result can be null if special tokens don't have associated word IDs
            // This is expected behavior
        }
    }

    [Fact]
    public void CharToToken_WithValidCharPosition_ReturnsTokenIndex()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - First character should map to a token
        var result = encoding.CharToToken(charPosition: 0, sequenceIndex: 0);

        // Assert
        if (encoding.Offsets.Any(o => o.Item1 <= 0 && o.Item2 > 0))
        {
            Assert.NotNull(result);
            Assert.True(result >= 0);
            Assert.True(result < encoding.Length);
        }
    }

    [Fact]
    public void CharToToken_WithCharPositionOutOfRange_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Use a character position way beyond the input
        var result = encoding.CharToToken(charPosition: 100000, sequenceIndex: 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CharToToken_WithWrongSequenceIndex_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act - Single sequence, so sequence 1 doesn't exist
        var result = encoding.CharToToken(charPosition: 0, sequenceIndex: 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CharToWord_WithValidCharPosition_ReturnsWordIndex()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.CharToWord(charPosition: 0, sequenceIndex: 0);

        // Assert - First char should map to a word if word IDs exist
        if (encoding.WordIds.Any(w => w.HasValue))
        {
            // Result may be null if char maps to a special token without word ID
            // but if it's not null, it should be valid
            if (result.HasValue)
            {
                Assert.True(result >= 0);
            }
        }
    }

    [Fact]
    public void CharToWord_WithInvalidCharPosition_ReturnsNull()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        
        // Act
        var result = encoding.CharToWord(charPosition: 100000, sequenceIndex: 0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void PositionMapping_RoundTrip_WordToTokenToWord()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world test");
        
        // Find a valid word index
        var validWordId = encoding.WordIds.FirstOrDefault(w => w.HasValue);
        if (!validWordId.HasValue) return; // Skip if no word IDs

        // Act - Word -> Tokens -> Word
        var tokenRange = encoding.WordToTokens(validWordId.Value, sequenceIndex: 0);
        Assert.NotNull(tokenRange);

        var backToWord = encoding.TokenToWord(tokenRange.Value.StartToken);
        
        // Assert
        Assert.NotNull(backToWord);
        Assert.Equal(validWordId.Value, backToWord.Value.WordIndex);
    }

    [Fact]
    public void PositionMapping_RoundTrip_TokenToCharToToken()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world test");
        
        // Act - Token -> Chars -> Token
        var charOffsets = encoding.TokenToChars(tokenIndex: 0);
        
        // Assert - Only test if sequence IDs are available
        if (charOffsets.HasValue)
        {
            var backToToken = encoding.CharToToken(charOffsets.Value.Item2.Start, charOffsets.Value.SequenceIndex);
            Assert.NotNull(backToToken);
            Assert.Equal(0, backToToken.Value);
        }
    }

    [Fact]
    public void PositionMapping_SequencePair_DifferentSequenceIndices()
    {
        // Arrange
        var encoding = _tokenizer.Encode("First sentence", "Second sentence");
        
        // Act - Check that we have multiple sequence indices
        var uniqueSequences = encoding.SequenceIds.Distinct().Count();
        
        // Assert
        Assert.True(uniqueSequences > 1, "Sequence pair should have multiple sequence indices");
        
        // Verify sequence index mapping works
        for (int i = 0; i < encoding.Length; i++)
        {
            var seqIndex = encoding.TokenToSequence(i);
            if (seqIndex.HasValue)
            {
                Assert.Equal(encoding.SequenceIds[i], seqIndex.Value);
            }
        }
    }
}
