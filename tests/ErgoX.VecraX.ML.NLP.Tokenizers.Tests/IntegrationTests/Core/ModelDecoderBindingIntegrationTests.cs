namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Core;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
public sealed class ModelDecoderBindingIntegrationTests : IDisposable
{
    private readonly Tokenizer _tokenizer;

    public ModelDecoderBindingIntegrationTests()
    {
        _tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void TokenizerModel_FromJson_ExposesTypeAndRoundtrips()
    {
        using var model = TokenizerModel.FromJson(ExtractModelJson(_tokenizer));

        Assert.Equal("BPE", model.Type);

        var serialized = model.ToJson();
        using var roundtrip = JsonDocument.Parse(serialized);
        Assert.Equal("BPE", roundtrip.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void TokenizerModel_FromJson_RequiresContent()
    {
        Assert.Throws<ArgumentException>(() => TokenizerModel.FromJson(string.Empty));
        Assert.Throws<ArgumentException>(() => TokenizerModel.FromJson("   "));
    }

    [Fact]
    public void BpeModel_FromFiles_ExposesTypeAndOptions()
    {
        var root = TestDataPath.GetModelRoot("gpt2");
        var vocabPath = Path.Combine(root, "vocab.json");
        var mergesPath = Path.Combine(root, "merges.txt");
        using var model = new BpeModel(vocabPath, mergesPath, new BpeModelOptions { Dropout = 0.15f, FuseUnknownTokens = true });

        Assert.Equal("BPE", model.Type);

        var serialized = JsonDocument.Parse(model.ToJson()).RootElement;
        Assert.True(serialized.TryGetProperty("dropout", out var dropout));
        Assert.Equal(0.15, dropout.GetDouble(), 2);
        Assert.True(serialized.GetProperty("fuse_unk").GetBoolean());
    }

    [Fact]
    public void WordPieceModel_FromFile_AppliesConfiguration()
    {
        var root = TestDataPath.GetModelRoot("bert-base-uncased");
        var vocabPath = Path.Combine(root, "vocab.txt");
        using var model = new WordPieceModel(vocabPath, new WordPieceModelOptions { ContinuingSubwordPrefix = "@@", MaxInputCharsPerWord = 150 });

        Assert.Equal("WordPiece", model.Type);

        var serialized = JsonDocument.Parse(model.ToJson()).RootElement;
        Assert.Equal("@@", serialized.GetProperty("continuing_subword_prefix").GetString());
        Assert.Equal(150, serialized.GetProperty("max_input_chars_per_word").GetInt32());
    }

    [Fact]
    public void UnigramModel_FromFile_ReportsType()
    {
        var root = TestDataPath.GetModelRoot("google-mt5-small");
        var tokenizerPath = Path.Combine(root, "tokenizer.json");
        using var document = JsonDocument.Parse(File.ReadAllText(tokenizerPath));
        var modelJson = document.RootElement.GetProperty("model").GetRawText();
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            File.WriteAllText(tempPath, modelJson);
            using var model = new UnigramModel(tempPath);

            Assert.Equal("Unigram", model.Type);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void Tokenizer_SetModel_ReplacesPayload()
    {
        using var target = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("bert-base-uncased"));
        using var source = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));

        var mutated = MutateModelDropout(ExtractModelJson(source), 0.1);
        using var model = TokenizerModel.FromJson(mutated);

        target.SetModel(model);

        var updated = target.ToJson();
        using var document = JsonDocument.Parse(updated);
        var modelElement = document.RootElement.GetProperty("model");

        Assert.Equal("BPE", modelElement.GetProperty("type").GetString());
        Assert.True(modelElement.TryGetProperty("dropout", out var dropout));
        Assert.Equal(0.1, dropout.GetDouble(), 3);
    }

    [Fact]
    public void Tokenizer_SetModel_AllowsWordPieceModel()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var root = TestDataPath.GetModelRoot("bert-base-uncased");
        var vocabPath = Path.Combine(root, "vocab.txt");
        using var model = new WordPieceModel(vocabPath);

        tokenizer.SetModel(model);

        using var document = JsonDocument.Parse(tokenizer.ToJson());
        var modelElement = document.RootElement.GetProperty("model");
        Assert.Equal("WordPiece", modelElement.GetProperty("type").GetString());
    }

    [Fact]
    public void Tokenizer_SetModel_RequiresKnownImplementation()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        using var unknown = new FakeModel();

        Assert.Throws<ArgumentException>(() => tokenizer.SetModel(unknown));
    }

    [Fact]
    public void TokenizerDecoder_FromJson_ExposesTypeAndRoundtrips()
    {
        using var decoder = TokenizerDecoder.FromJson(ExtractDecoderJson(_tokenizer));

        Assert.Equal("ByteLevel", decoder.Type);

        var serialized = decoder.ToJson();
        using var roundtrip = JsonDocument.Parse(serialized);
        Assert.Equal("ByteLevel", roundtrip.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void TokenizerDecoder_FromJson_RequiresContent()
    {
        Assert.Throws<ArgumentException>(() => TokenizerDecoder.FromJson(string.Empty));
        Assert.Throws<ArgumentException>(() => TokenizerDecoder.FromJson("\t"));
    }

    [Fact]
    public void Tokenizer_SetDecoder_ReplacesAndClears()
    {
        using var target = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var mutated = MutateDecoderPrefixSpace(ExtractDecoderJson(target), out var expectedPrefixSpace);
        using var decoder = TokenizerDecoder.FromJson(mutated);

        target.SetDecoder(decoder);

        var updated = target.ToJson();
        using (var document = JsonDocument.Parse(updated))
        {
            var decoderElement = document.RootElement.GetProperty("decoder");
            Assert.Equal("ByteLevel", decoderElement.GetProperty("type").GetString());
            Assert.Equal(expectedPrefixSpace, decoderElement.GetProperty("add_prefix_space").GetBoolean());
        }

        target.ClearDecoder();
        var cleared = target.ToJson();
        using var clearedDocument = JsonDocument.Parse(cleared);
        Assert.Equal(JsonValueKind.Null, clearedDocument.RootElement.GetProperty("decoder").ValueKind);
    }

    [Fact]
    public void Tokenizer_SetDecoder_RequiresKnownImplementation()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        using var unknown = new FakeDecoder();

        Assert.Throws<ArgumentException>(() => tokenizer.SetDecoder(unknown));
    }

    [Fact]
    public void TokenizerDecoder_Dispose_IsIdempotent()
    {
        var decoderJson = ExtractDecoderJson(_tokenizer);
        var decoder = TokenizerDecoder.FromJson(decoderJson);

        try
        {
            var ex = Record.Exception(() =>
            {
                decoder.Dispose();
                decoder.Dispose();
            });

            Assert.Null(ex);
        }
        finally
        {
            decoder.Dispose();
        }
    }

    private static string ExtractModelJson(Tokenizer tokenizer)
    {
        var json = tokenizer.ToJson();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("model").GetRawText();
    }

    private static string ExtractDecoderJson(Tokenizer tokenizer)
    {
        var json = tokenizer.ToJson();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("decoder").GetRawText();
    }

    private static string MutateModelDropout(string json, double dropout)
    {
        var node = JsonNode.Parse(json) as JsonObject ?? throw new InvalidOperationException("Model payload must be an object.");
        node["dropout"] = dropout;
        return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static string MutateDecoderPrefixSpace(string json, out bool newValue)
    {
        var node = JsonNode.Parse(json) as JsonObject ?? throw new InvalidOperationException("Decoder payload must be an object.");
        var current = false;
        if (node.TryGetPropertyValue("add_prefix_space", out var prefixSpace) && prefixSpace is JsonValue flag && flag.TryGetValue<bool>(out var existing))
        {
            current = existing;
        }

        newValue = !current;
        node["add_prefix_space"] = newValue;
        return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private sealed class FakeModel : IModel
    {
        public IntPtr Handle => IntPtr.Zero;

        public void Dispose()
        {
        }
    }

    private sealed class FakeDecoder : IDecoder
    {
        public IntPtr Handle => IntPtr.Zero;

        public void Dispose()
        {
        }
    }
}
