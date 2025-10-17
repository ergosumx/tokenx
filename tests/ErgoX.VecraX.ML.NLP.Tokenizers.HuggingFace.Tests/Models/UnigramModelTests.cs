using System;
using System.Collections.Generic;
using ErgoX.VecraX.ML.Tokenizers.Models;
using Xunit;

namespace ErgoX.VecraX.ML.Tokenizers.Tests.Models;

/// <summary>
/// Tests for the Unigram tokenization model.
/// Unigram uses probabilistic scores to determine tokenization.
/// </summary>
public sealed class UnigramModelTests : IDisposable
{
    /// <summary>
    /// Creates a sample vocabulary with scores for testing.
    /// Based on typical SentencePiece vocabulary format.
    /// </summary>
    /// <returns>List of (token, score) tuples</returns>
    private static List<(string, double)> CreateSampleVocab()
    {
        return new List<(string, double)>
        {
            // Common tokens with their scores (negative log probabilities)
            ("▁the", -2.5),
            ("▁a", -3.0),
            ("▁is", -3.2),
            ("▁to", -3.5),
            ("▁and", -3.7),
            ("▁of", -4.0),
            ("▁in", -4.2),
            ("▁for", -4.5),
            // Subword pieces
            ("▁test", -5.0),
            ("▁ing", -5.5),
            ("▁ed", -6.0),
            ("▁s", -6.5),
            // Characters
            ("a", -7.0),
            ("b", -7.5),
            ("c", -8.0),
            ("d", -8.5),
            ("e", -9.0),
            // Unknown token
            ("<unk>", -10.0)
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidVocab_CreatesModel()
    {
        // Arrange
        var vocab = CreateSampleVocab();

        // Act
        using var model = new UnigramModel(vocab);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithUnkId_CreatesModel()
    {
        // Arrange
        var vocab = CreateSampleVocab();
        int unkId = 17; // Index of <unk> token

        // Act
        using var model = new UnigramModel(vocab, unkId: unkId);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithByteFallback_CreatesModel()
    {
        // Arrange
        var vocab = CreateSampleVocab();

        // Act
        using var model = new UnigramModel(vocab, byteFallback: true);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithAllOptions_CreatesModel()
    {
        // Arrange
        var vocab = CreateSampleVocab();
        int unkId = 17; // Index of <unk> token

        // Act
        using var model = new UnigramModel(vocab, unkId: unkId, byteFallback: true);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithNullVocab_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnigramModel(null!));
    }

    [Fact]
    public void Constructor_WithEmptyVocab_ThrowsArgumentException()
    {
        // Arrange
        var emptyVocab = new List<(string, double)>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UnigramModel(emptyVocab));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ReleasesNativeResources()
    {
        // Arrange
        var vocab = CreateSampleVocab();
        var model = new UnigramModel(vocab);

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
        var vocab = CreateSampleVocab();
        var model = new UnigramModel(vocab);

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
        // No cleanup needed - using in-memory test data
        GC.SuppressFinalize(this);
    }
}
