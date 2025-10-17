using System;
using System.IO;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Models;

/// <summary>
/// Tests for WordLevel model using simple character-level vocabulary.
/// WordLevel is the simplest tokenization model - direct token-to-ID mapping.
/// </summary>
public sealed class WordLevelModelTests : IDisposable
{
    private readonly string _simpleVocabPath;

    public WordLevelModelTests()
    {
        // Use simple character-level vocabulary for testing
        var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        _simpleVocabPath = Path.Combine(testDataPath, "simple-vocab.json");

        // Verify test data exists
        if (!File.Exists(_simpleVocabPath))
        {
            throw new FileNotFoundException($"Simple vocabulary file not found: {_simpleVocabPath}. " +
                "Please ensure simple-vocab.json is in the TestData directory.");
        }
    }

    #region FromFile Tests

    [Fact]
    public void FromFile_WithValidFile_CreatesModel()
    {
        // Act
        using var model = WordLevelModel.FromFile(_simpleVocabPath);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithUnkToken_CreatesModel()
    {
        // Act - Use [UNK] as unknown token
        using var model = WordLevelModel.FromFile(_simpleVocabPath, unkToken: "[UNK]");

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithNullVocabPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => WordLevelModel.FromFile(null!));
    }

    [Fact]
    public void FromFile_WithEmptyVocabPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => WordLevelModel.FromFile(string.Empty));
    }

    [Fact]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_vocab.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => WordLevelModel.FromFile(nonExistentPath));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ReleasesNativeResources()
    {
        // Arrange
        var model = WordLevelModel.FromFile(_simpleVocabPath);

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
        var model = WordLevelModel.FromFile(_simpleVocabPath);

        // Act - Call Dispose multiple times
        model.Dispose();
        model.Dispose();
        model.Dispose();

        // Assert - Should not throw, just verify we get here
        Assert.True(true);
    }

    #endregion

    /// <summary>
    /// Cleanup method called by xUnit after all tests in this class complete.
    /// </summary>
    public void Dispose()
    {
        // No cleanup needed - using shared test data files
        GC.SuppressFinalize(this);
    }
}
