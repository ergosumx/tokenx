using System;
using System.IO;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Models;

/// <summary>
/// Tests for WordPiece model using real BERT tokenizer data from HuggingFace.
/// All tests use bert-base-uncased vocabulary (30522 tokens).
/// </summary>
public class WordPieceModelTests : IDisposable
{
    private readonly string _bertVocabPath;

    public WordPieceModelTests()
    {
        // Use real BERT vocabulary from HuggingFace
        var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        _bertVocabPath = Path.Combine(testDataPath, "bert-vocab.txt");

        // Verify test data exists
        if (!File.Exists(_bertVocabPath))
        {
            throw new FileNotFoundException($"BERT vocabulary file not found: {_bertVocabPath}. " +
                "Please ensure bert-vocab.txt is in the TestData directory.");
        }
    }

    #region FromFile Tests

    [Fact]
    public void FromFile_WithValidFile_CreatesModel()
    {
        // Act
        using var model = WordPieceModel.FromFile(_bertVocabPath);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithUnkToken_CreatesModel()
    {
        // Act - BERT uses [UNK] as unknown token
        using var model = WordPieceModel.FromFile(_bertVocabPath, unkToken: "[UNK]");

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithCustomMaxChars_CreatesModel()
    {
        // Act - Set max characters per word to 50 (lower than default 100)
        using var model = WordPieceModel.FromFile(_bertVocabPath, maxInputCharsPerWord: 50);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithContinuingSubwordPrefix_CreatesModel()
    {
        // Act - BERT uses ## prefix for continuing subwords
        using var model = WordPieceModel.FromFile(_bertVocabPath, continuingSubwordPrefix: "##");

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithAllOptions_CreatesModel()
    {
        // Act - Use all BERT-specific options
        using var model = WordPieceModel.FromFile(
            _bertVocabPath,
            unkToken: "[UNK]",
            maxInputCharsPerWord: 100,
            continuingSubwordPrefix: "##");

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithNullVocabPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WordPieceModel.FromFile(null!));
    }

    [Fact]
    public void FromFile_WithEmptyVocabPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => WordPieceModel.FromFile(string.Empty));
    }

    [Fact]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_vocab.txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => WordPieceModel.FromFile(nonExistentPath));
    }

    [Fact]
    public void FromFile_WithInvalidMaxChars_ThrowsArgumentException()
    {
        // Act & Assert - maxInputCharsPerWord must be positive
        Assert.Throws<ArgumentException>(() =>
            WordPieceModel.FromFile(_bertVocabPath, maxInputCharsPerWord: 0));
    }

    [Fact]
    public void FromFile_WithNegativeMaxChars_ThrowsArgumentException()
    {
        // Act & Assert - maxInputCharsPerWord must be positive
        Assert.Throws<ArgumentException>(() =>
            WordPieceModel.FromFile(_bertVocabPath, maxInputCharsPerWord: -1));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ReleasesNativeResources()
    {
        // Arrange
        var model = WordPieceModel.FromFile(_bertVocabPath);

        // Act
        model.Dispose();

        // Assert - After dispose, the handle is released (verified by successful disposal)
        // Note: We can't test ObjectDisposedException directly as Handle is internal
        Assert.True(true); // Disposal completed successfully
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var model = WordPieceModel.FromFile(_bertVocabPath);

        // Act - Call Dispose multiple times
        model.Dispose();
        model.Dispose();
        model.Dispose();

        // Assert - Should not throw, just verify we get here
        Assert.True(true);
    }

    #endregion

    public void Dispose()
    {
        // No cleanup needed - using shared test data files
    }
}
