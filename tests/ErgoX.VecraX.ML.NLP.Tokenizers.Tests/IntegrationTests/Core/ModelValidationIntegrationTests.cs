namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Core;

using System;
using System.IO;
using System.Text.Json;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
public static class ModelValidationIntegrationTests
{
    [Fact]
    public static void BpeModel_InvalidDropout_Throws()
    {
        var root = TestDataPath.GetModelRoot("gpt2");
        var vocabPath = Path.Combine(root, "vocab.json");
        var mergesPath = Path.Combine(root, "merges.txt");

        Assert.Throws<ArgumentOutOfRangeException>(() => new BpeModel(vocabPath, mergesPath, new BpeModelOptions { Dropout = -0.01f }));
    }

    [Fact]
    public static void BpeModel_MissingVocabPath_Throws()
    {
        var root = TestDataPath.GetModelRoot("gpt2");
        var mergesPath = Path.Combine(root, "merges.txt");

        Assert.Throws<ArgumentException>(() => new BpeModel(string.Empty, mergesPath));
    }

    [Fact]
    public static void WordPieceModel_BlankUnknownToken_Throws()
    {
        var root = TestDataPath.GetModelRoot("bert-base-uncased");
        var vocabPath = Path.Combine(root, "vocab.txt");

        Assert.Throws<ArgumentException>(() => new WordPieceModel(vocabPath, new WordPieceModelOptions { UnknownToken = "  " }));
    }

    [Fact]
    public static void WordPieceModel_NonPositiveMaxChars_Throws()
    {
        var root = TestDataPath.GetModelRoot("bert-base-uncased");
        var vocabPath = Path.Combine(root, "vocab.txt");

        Assert.Throws<ArgumentOutOfRangeException>(() => new WordPieceModel(vocabPath, new WordPieceModelOptions { MaxInputCharsPerWord = 0 }));
    }

    [Fact]
    public static void UnigramModel_MissingPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => new UnigramModel(" "));
    }

    [Fact]
    public static void TokenizerDecoder_AccessAfterDispose_Throws()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var tokenizerJson = tokenizer.ToJson();
        using var document = JsonDocument.Parse(tokenizerJson);
        var decoderJson = document.RootElement.GetProperty("decoder").GetRawText();

        var decoder = TokenizerDecoder.FromJson(decoderJson);
        decoder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => decoder.ToJson());
    }
}
