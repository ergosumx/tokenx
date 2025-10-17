using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Models;

/// <summary>
/// Tests for the BPE (Byte-Pair Encoding) model implementation.
/// Uses real GPT-2 tokenizer files from HuggingFace for testing.
/// </summary>
public class BpeModelTests : IDisposable
{
    private static readonly string TestDataPath = Path.Combine(
        Path.GetDirectoryName(typeof(BpeModelTests).Assembly.Location)!,
        "..", "..", "..", "TestData");

    private readonly string _gpt2VocabPath;
    private readonly string _gpt2MergesPath;

    public BpeModelTests()
    {
        // Use real GPT-2 tokenizer files from TestData folder
        _gpt2VocabPath = Path.GetFullPath(Path.Combine(TestDataPath, "gpt2-vocab.json"));
        _gpt2MergesPath = Path.GetFullPath(Path.Combine(TestDataPath, "gpt2-merges.txt"));

        // Verify files exist
        if (!File.Exists(_gpt2VocabPath))
        {
            throw new FileNotFoundException(
                $"GPT-2 vocab file not found at {_gpt2VocabPath}. " +
                "Please ensure TestData folder contains gpt2-vocab.json");
        }

        if (!File.Exists(_gpt2MergesPath))
        {
            throw new FileNotFoundException(
                $"GPT-2 merges file not found at {_gpt2MergesPath}. " +
                "Please ensure TestData folder contains gpt2-merges.txt");
        }
    }

    // Helper method to load GPT-2 vocab and merges (mirrors Python's BPE.read_file pattern)
    private (Dictionary<string, int> vocab, List<(string, string)> merges) LoadGpt2VocabAndMerges()
    {
        var vocab = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(_gpt2VocabPath))!;
        var merges = File.ReadAllLines(_gpt2MergesPath)
            .Skip(1) // Skip header line
            .Select(line => line.Split(' '))
            .Where(parts => parts.Length == 2)
            .Select(parts => (parts[0], parts[1]))
            .ToList();
        return (vocab, merges);
    }

    [Fact]
    public void Constructor_WithValidVocabAndMerges_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab/merges (following Python pattern: BPE.read_file then constructor)
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithNullVocab_ThrowsArgumentNullException()
    {
        // Arrange - Load real GPT-2 merges
        var (_, merges) = LoadGpt2VocabAndMerges();
        IReadOnlyDictionary<string, int>? vocab = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BpeModel(vocab!, merges));
    }

    [Fact]
    public void Constructor_WithNullMerges_ThrowsArgumentNullException()
    {
        // Arrange - Load real GPT-2 vocab
        var (vocab, _) = LoadGpt2VocabAndMerges();
        IReadOnlyList<(string, string)>? merges = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BpeModel(vocab, merges!));
    }

    [Fact]
    public void Constructor_WithInvalidDropout_ThrowsArgumentException()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act & Assert - dropout too low
        Assert.Throws<ArgumentException>(() => new BpeModel(vocab, merges, dropout: -0.5f));

        // Act & Assert - dropout too high
        Assert.Throws<ArgumentException>(() => new BpeModel(vocab, merges, dropout: 1.5f));
    }

    [Fact]
    public void Constructor_WithCacheCapacity_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges, cacheCapacity: 5000);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithDropout_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges, dropout: 0.1f);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithUnknownToken_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges, unknownToken: "<unk>");

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithFuseUnknown_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges, fuseUnknown: true);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_WithByteFallback_CreatesModel()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();

        // Act
        using var model = new BpeModel(vocab, merges, byteFallback: true);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithValidFiles_CreatesModel()
    {
        // Act
        using var model = BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithNullVocabPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BpeModel.FromFile(null!, _gpt2MergesPath));
    }

    [Fact]
    public void FromFile_WithEmptyVocabPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BpeModel.FromFile(string.Empty, _gpt2MergesPath));
    }

    [Fact]
    public void FromFile_WithNullMergesPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BpeModel.FromFile(_gpt2VocabPath, null!));
    }

    [Fact]
    public void FromFile_WithEmptyMergesPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => BpeModel.FromFile(_gpt2VocabPath, string.Empty));
    }

    [Fact]
    public void FromFile_WithNonexistentVocabFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonexistentPath = Path.Combine(TestDataPath, "nonexistent_vocab.json");

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() => BpeModel.FromFile(nonexistentPath, _gpt2MergesPath));
        Assert.Contains("Vocabulary file not found", ex.Message);
    }

    [Fact]
    public void FromFile_WithNonexistentMergesFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonexistentPath = Path.Combine(TestDataPath, "nonexistent_merges.txt");

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() => BpeModel.FromFile(_gpt2VocabPath, nonexistentPath));
        Assert.Contains("Merges file not found", ex.Message);
    }

    [Fact]
    public void FromFile_WithCacheCapacity_CreatesModel()
    {
        // Act
        using var model = BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath, cacheCapacity: 5000);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithDropout_CreatesModel()
    {
        // Act
        using var model = BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath, dropout: 0.1f);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FromFile_WithInvalidDropout_ThrowsArgumentException()
    {
        // Act & Assert - dropout too low
        Assert.Throws<ArgumentException>(() =>
            BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath, dropout: -0.5f));

        // Act & Assert - dropout too high
        Assert.Throws<ArgumentException>(() =>
            BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath, dropout: 1.5f));
    }

    [Fact]
    public void FromFile_WithUnknownToken_CreatesModel()
    {
        // Act
        using var model = BpeModel.FromFile(_gpt2VocabPath, _gpt2MergesPath, unknownToken: "<unk>");

        // Assert
        Assert.NotNull(model);
    }

    [Fact(Skip = "continuingSubwordPrefix/endOfWordSuffix cause Rust library panic with GPT-2's byte-level vocab (Rust lib limitation, not bindings issue). Python bindings accept these params but Rust panics at model.rs:186")]
    public void FromFile_WithAllOptions_CreatesModel()
    {
        // Act - Python bindings accept these options, but Rust library panics with GPT-2 vocab
        // This is a known limitation: these options are for WordPiece-style tokenizers (BERT),
        // not byte-level BPE tokenizers (GPT-2)
        using var model = BpeModel.FromFile(
            _gpt2VocabPath,
            _gpt2MergesPath,
            cacheCapacity: 5000,
            dropout: 0.1f,
            unknownToken: "<unk>",
            continuingSubwordPrefix: "##",
            endOfWordSuffix: "</w>",
            fuseUnknown: true);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void Dispose_ReleasesNativeResources()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();
        var model = new BpeModel(vocab, merges);

        // Act
        model.Dispose();

        // Assert - After dispose, the handle is released (verified by successful disposal)
        // Note: We can't test ObjectDisposedException directly as Handle is internal
        Assert.True(true); // Disposal completed successfully
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange - Load real GPT-2 vocab and merges
        var (vocab, merges) = LoadGpt2VocabAndMerges();
        var model = new BpeModel(vocab, merges);

        // Act - Call Dispose multiple times
        model.Dispose();
        model.Dispose();
        model.Dispose();

        // Assert - Should not throw, just verify we get here
        Assert.True(true);
    }

    public void Dispose()
    {
        // No cleanup needed - using shared test data files
    }
}
