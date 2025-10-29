namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using Xunit;

/// <summary>
/// Tests for encoding manipulation methods (Merge, Pad, Truncate).
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public class EncodingManipulationIntegrationTests : IDisposable
{
    private const string SolutionFileName = "TokenX.sln";
    private readonly Tokenizer _tokenizer;

    public EncodingManipulationIntegrationTests()
    {
        _tokenizer = Tokenizer.FromFile(GetTokenizerPath());
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
    }

    [Fact]
    public void Merge_WithEmptyList_ReturnsNull()
    {
        // Arrange
        var encodings = new List<EncodingResult>();

        // Act
        var result = EncodingResult.Merge(encodings, growingOffsets: true);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Merge_WithSingleEncoding_ReturnsSameEncoding()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        var encodings = new List<EncodingResult> { encoding };

        // Act
        var result = EncodingResult.Merge(encodings, growingOffsets: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(encoding.Ids, result.Ids);
        Assert.Equal(encoding.Tokens, result.Tokens);
        Assert.Equal(encoding.Offsets, result.Offsets);
    }

    [Fact]
    public void Merge_WithMultipleEncodings_CombinesIdsAndTokens()
    {
        // Arrange
        var encoding1 = _tokenizer.Encode("Hello");
        var encoding2 = _tokenizer.Encode("world");
        var encodings = new List<EncodingResult> { encoding1, encoding2 };

        // Act
        var result = EncodingResult.Merge(encodings, growingOffsets: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(encoding1.Length + encoding2.Length, result.Length);
        Assert.Equal(encoding1.Ids.Concat(encoding2.Ids), result.Ids);
        Assert.Equal(encoding1.Tokens.Concat(encoding2.Tokens), result.Tokens);
    }

    [Fact]
    public void Merge_WithGrowingOffsets_AdjustsOffsetsCorrectly()
    {
        // Arrange
        var encoding1 = _tokenizer.Encode("Hello");
        var encoding2 = _tokenizer.Encode("world");
        var encodings = new List<EncodingResult> { encoding1, encoding2 };

        // Get the last offset from encoding1 to calculate expected adjustment
        var lastOffset = encoding1.Offsets.Last();

        // Act
        var result = EncodingResult.Merge(encodings, growingOffsets: true);

        // Assert
        Assert.NotNull(result);
        // First encoding's offsets should be unchanged
        for (int i = 0; i < encoding1.Length; i++)
        {
            Assert.Equal(encoding1.Offsets[i], result.Offsets[i]);
        }
        // Second encoding's offsets should be adjusted
        for (int i = 0; i < encoding2.Length; i++)
        {
            var expected = (encoding2.Offsets[i].Item1 + lastOffset.Item2,
                           encoding2.Offsets[i].Item2 + lastOffset.Item2);
            Assert.Equal(expected, result.Offsets[encoding1.Length + i]);
        }
    }

    [Fact]
    public void Merge_WithoutGrowingOffsets_PreservesOriginalOffsets()
    {
        // Arrange
        var encoding1 = _tokenizer.Encode("Hello");
        var encoding2 = _tokenizer.Encode("world");
        var encodings = new List<EncodingResult> { encoding1, encoding2 };

        // Act
        var result = EncodingResult.Merge(encodings, growingOffsets: false);

        // Assert
        Assert.NotNull(result);
        var expectedOffsets = encoding1.Offsets.Concat(encoding2.Offsets);
        Assert.Equal(expectedOffsets, result.Offsets);
    }

    [Fact]
    public void Pad_WithTargetLengthLessThanCurrent_ReturnsUnchanged()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a test");
        var originalLength = encoding.Length;

        // Act
        var result = encoding.Pad(targetLength: originalLength - 2, padId: 0, padTypeId: 0, padToken: "[PAD]", direction: PaddingDirection.Right);

        // Assert
        Assert.Equal(originalLength, result.Length);
        Assert.Equal(encoding.Ids, result.Ids);
    }

    [Fact]
    public void Pad_WithTargetLengthEqualToCurrent_ReturnsUnchanged()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");
        var originalLength = encoding.Length;

        // Act
        var result = encoding.Pad(targetLength: originalLength, padId: 0, padTypeId: 0, padToken: "[PAD]", direction: PaddingDirection.Right);

        // Assert
        Assert.Equal(originalLength, result.Length);
        Assert.Equal(encoding.Ids, result.Ids);
    }

    [Fact]
    public void Pad_WithRightDirection_AppendsPaddingTokens()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello");
        var originalLength = encoding.Length;
        var targetLength = originalLength + 5;
        var padId = 0;
        var padToken = "[PAD]";

        // Act
        var result = encoding.Pad(targetLength, padId, padTypeId: 0, padToken, direction: PaddingDirection.Right);

        // Assert
        Assert.Equal(targetLength, result.Length);
        // Original tokens should be at the start
        for (int i = 0; i < originalLength; i++)
        {
            Assert.Equal(encoding.Ids[i], result.Ids[i]);
        }
        // Padding tokens should be at the end
        for (int i = originalLength; i < targetLength; i++)
        {
            Assert.Equal(padId, result.Ids[i]);
            Assert.Equal(padToken, result.Tokens[i]);
        }
    }

    [Fact]
    public void Pad_WithLeftDirection_PrependsPaddingTokens()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello");
        var originalLength = encoding.Length;
        var targetLength = originalLength + 5;
        var padId = 0;
        var padToken = "[PAD]";

        // Act
        var result = encoding.Pad(targetLength, padId, padTypeId: 0, padToken, direction: PaddingDirection.Left);

        // Assert
        Assert.Equal(targetLength, result.Length);
        // Padding tokens should be at the start
        var paddingCount = targetLength - originalLength;
        for (int i = 0; i < paddingCount; i++)
        {
            Assert.Equal(padId, result.Ids[i]);
            Assert.Equal(padToken, result.Tokens[i]);
        }
        // Original tokens should be at the end
        for (int i = 0; i < originalLength; i++)
        {
            Assert.Equal(encoding.Ids[i], result.Ids[paddingCount + i]);
        }
    }

    [Fact]
    public void Pad_UpdatesAttentionMaskCorrectly()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello");
        var originalLength = encoding.Length;
        var targetLength = originalLength + 3;

        // Act
        var result = encoding.Pad(targetLength, padId: 0, padTypeId: 0, padToken: "[PAD]", direction: PaddingDirection.Right);

        // Assert
        Assert.NotNull(result.AttentionMask);
        Assert.Equal(targetLength, result.AttentionMask.Count);
        // Original tokens should have attention mask = 1
        Assert.All(result.AttentionMask.Take(originalLength), mask => Assert.Equal(1u, mask));
        // Padding tokens should have attention mask = 0
        Assert.All(result.AttentionMask.Skip(originalLength), mask => Assert.Equal(0u, mask));
    }

    [Fact]
    public void Truncate_WithMaxLengthGreaterThanCurrent_ReturnsUnchanged()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello");
        var originalLength = encoding.Length;

        // Act
        var result = encoding.Truncate(maxLength: originalLength + 5, stride: 0, direction: TruncationDirection.Right);

        // Assert
        Assert.Equal(originalLength, result.Length);
        Assert.Equal(encoding.Ids, result.Ids);
        Assert.Empty(result.Overflowing);
    }

    [Fact]
    public void Truncate_WithRightDirection_RemovesFromEnd()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a test");
        var maxLength = 5;

        // Act
        var result = encoding.Truncate(maxLength, stride: 0, direction: TruncationDirection.Right);

        // Assert
        Assert.Equal(maxLength, result.Length);
        Assert.Equal(encoding.Ids.Take(maxLength), result.Ids);
        Assert.Equal(encoding.Tokens.Take(maxLength), result.Tokens);
    }

    [Fact]
    public void Truncate_WithLeftDirection_RemovesFromStart()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a test");
        var originalLength = encoding.Length;
        var maxLength = 5;

        // Act
        var result = encoding.Truncate(maxLength, stride: 0, direction: TruncationDirection.Left);

        // Assert
        Assert.Equal(maxLength, result.Length);
        Assert.Equal(encoding.Ids.Skip(originalLength - maxLength), result.Ids);
        Assert.Equal(encoding.Tokens.Skip(originalLength - maxLength), result.Tokens);
    }

    [Fact]
    public void Truncate_WithStride_GeneratesOverflowingEncodings()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a long test sentence with many tokens");
        var maxLength = 8;
        var stride = 2;

        // Act
        var result = encoding.Truncate(maxLength, stride, direction: TruncationDirection.Right);

        // Assert
        Assert.Equal(maxLength, result.Length);
        Assert.NotEmpty(result.Overflowing);

        // First overflowing should start at maxLength - stride
        var firstOverflow = result.Overflowing.First();
        Assert.True(firstOverflow.Length <= maxLength);
    }

    [Fact]
    public void Truncate_WithZeroStride_NoOverflowing()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a test");
        var maxLength = 5;

        // Act
        var result = encoding.Truncate(maxLength, stride: 0, direction: TruncationDirection.Right);

        // Assert
        Assert.Empty(result.Overflowing);
    }

    [Fact]
    public void Truncate_WithMaxLengthZero_ReturnsEmpty()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world");

        // Act
        var result = encoding.Truncate(maxLength: 0, stride: 0, direction: TruncationDirection.Right);

        // Assert
        Assert.Equal(0, result.Length);
        Assert.Empty(result.Ids);
        Assert.Empty(result.Tokens);
    }

    [Fact]
    public void Truncate_WithStrideLargerThanMaxLength_ThrowsException()
    {
        // Arrange
        var encoding = _tokenizer.Encode("Hello world this is a test sentence");
        var maxLength = 5;
        var stride = 10; // Larger than maxLength

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            encoding.Truncate(maxLength, stride, direction: TruncationDirection.Right));
    }

    private static string GetTokenizerPath()
    {
    var root = GetBenchmarksDataRoot();
        return Path.Combine(root, "bert-base-uncased", "tokenizer.json");
    }

    private static string GetBenchmarksDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionCandidate = Path.Combine(directory.FullName, SolutionFileName);
            if (File.Exists(solutionCandidate))
            {
                return Path.Combine(directory.FullName, "tests", "_huggingface");
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test context.");
    }
}

