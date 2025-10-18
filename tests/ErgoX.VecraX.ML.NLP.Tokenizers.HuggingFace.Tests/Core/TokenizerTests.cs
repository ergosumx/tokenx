using System;
using System.IO;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using Xunit;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;

public sealed class TokenizerTests
{
    private const string HelloToken = "Hello";
    private const string WorldToken = "World";
    private const string HelloWorldToken = "Hello World";
    private static readonly string[] HelloWorldTokens = { HelloToken, WorldToken };
    private static readonly int[] HelloWorldIds = { 1, 2 };
    private static readonly int[] HelloIds = { 1 };
    private static readonly int[] WorldIds = { 2 };

    private const string SampleTokenizerJson = """
    {
      "version": "1.0",
      "truncation": null,
      "padding": null,
      "added_tokens": [],
      "normalizer": null,
      "pre_tokenizer": { "type": "Whitespace" },
      "post_processor": null,
      "decoder": null,
      "model": {
        "type": "WordLevel",
        "vocab": {
          "[UNK]": 0,
          "Hello": 1,
          "World": 2
        },
        "unk_token": "[UNK]"
      }
    }
    """;

    [Fact]
    public void FromFileLoadsConfiguration()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            File.WriteAllText(path, SampleTokenizerJson);
            using var tokenizer = Tokenizer.FromFile(path);

            Assert.Equal(HelloWorldIds, tokenizer.Encode(HelloWorldToken).Ids);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void FromBufferParsesConfiguration()
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(SampleTokenizerJson);
        using var tokenizer = Tokenizer.FromBuffer(buffer);

        Assert.Equal(WorldIds, tokenizer.Encode(WorldToken).Ids);
    }

    [Fact]
    public void SaveWritesTokenizerJson()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            tokenizer.Save(path, pretty: true);
            Assert.True(File.Exists(path));

            var saved = File.ReadAllText(path);
            Assert.Contains("\"Hello\"", saved);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void EncodeProducesExpectedIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var encoding = tokenizer.Encode(HelloWorldToken);

        Assert.Equal(HelloWorldIds, encoding.Ids);
        Assert.Equal(HelloWorldTokens, encoding.Tokens);
        Assert.Equal(HelloWorldIds.Length, encoding.Length);
    }

    [Fact]
    public void EncodePairIncludesBothSequences()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var encoding = tokenizer.Encode(HelloToken, WorldToken);

        Assert.Equal(HelloIds.Concat(WorldIds), encoding.Ids);
    }

    [Fact]
    public void EncodeBatchProcessesSequences()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var batch = tokenizer.EncodeBatch(new[] { HelloToken, WorldToken });

        Assert.Collection(batch,
            first => Assert.Equal(HelloIds, first.Ids),
            second => Assert.Equal(WorldIds, second.Ids));
    }

    [Fact]
    public void EncodeBatchWithPairsProcessesSequences()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var batch = tokenizer.EncodeBatch(new[] { (HelloToken, (string?)null), (HelloToken, WorldToken) });

        Assert.Collection(batch,
            first => Assert.Equal(HelloIds, first.Ids),
            second => Assert.Equal(HelloIds.Concat(WorldIds), second.Ids));
    }

    [Fact]
    public void DecodeReturnsOriginalText()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var text = tokenizer.Decode(HelloWorldIds);

        Assert.Equal(HelloWorldToken, text);
    }

    [Fact]
    public void DecodeBatchReturnsAllTexts()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var texts = tokenizer.DecodeBatch(new[] { HelloIds, WorldIds });

        Assert.Collection(texts,
            first => Assert.Equal(HelloToken, first),
            second => Assert.Equal(WorldToken, second));
    }

    [Fact]
    public void PaddingOptionsRoundtrip()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);
        var options = new PaddingOptions(PaddingDirection.Left, padId: 99, padTypeId: 3, padToken: "[PAD]", length: 5, padToMultipleOf: 2);

        tokenizer.EnablePadding(options);
        var configured = tokenizer.GetPadding();

        Assert.NotNull(configured);
        Assert.Equal(PaddingDirection.Left, configured!.Direction);
        Assert.Equal(99u, configured.PadId);
        Assert.Equal(5, configured.Length);

        tokenizer.DisablePadding();
        Assert.Null(tokenizer.GetPadding());
    }

    [Fact]
    public void TruncationOptionsRoundtrip()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);
        var options = new TruncationOptions(maxLength: 3, stride: 1, TruncationStrategy.OnlyFirst, TruncationDirection.Right);

        tokenizer.EnableTruncation(options);
        var configured = tokenizer.GetTruncation();

        Assert.NotNull(configured);
        Assert.Equal(3, configured!.MaxLength);
        Assert.Equal(TruncationStrategy.OnlyFirst, configured.Strategy);

        tokenizer.DisableTruncation();
        Assert.Null(tokenizer.GetTruncation());
    }

    [Fact]
    public void TokenLookupReturnsExpectedValues()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        Assert.Equal(1, tokenizer.TokenToId(HelloToken));
        Assert.Equal("Hello", tokenizer.IdToToken(1));
        Assert.Null(tokenizer.TokenToId(string.Empty));
        Assert.Null(tokenizer.IdToToken(-1));
    }

    [Fact]
    public void ToJsonReturnsTokenizerConfiguration()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var json = tokenizer.ToJson();

        Assert.Contains("\"Hello\"", json);
        Assert.Contains("\"World\"", json);
    }
}
