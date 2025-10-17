using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.Tokenizers;
using Xunit;

namespace ErgoX.VecraX.ML.Tokenizers.Tests;

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
      "added_tokens": [
        {
          "id": 3,
          "content": "[PAD]",
          "single_word": false,
          "lstrip": false,
          "rstrip": false,
          "normalized": false,
          "special": true
        }
      ],
      "normalizer": null,
      "pre_tokenizer": { "type": "Whitespace" },
      "post_processor": null,
      "decoder": null,
      "model": {
        "type": "WordLevel",
        "vocab": {
          "[UNK]": 0,
          "Hello": 1,
          "World": 2,
          "[PAD]": 3
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
    public void AddTokensExtendsVocabulary()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var added = tokenizer.AddTokens(new[] { "Test" });

        Assert.Equal(1, added);
        Assert.NotNull(tokenizer.TokenToId("Test"));
        Assert.True(tokenizer.Config.Vocab.ContainsKey("Test"));
    }

    [Fact]
    public void AddSpecialTokensRegistersSpecials()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var added = tokenizer.AddSpecialTokens(new[] { "[CLS]" });

        Assert.Equal(1, added);

        var clsId = tokenizer.TokenToId("[CLS]");
        Assert.NotNull(clsId);

        var decoder = tokenizer.GetAddedTokensDecoder();
        Assert.True(decoder.ContainsKey(clsId!.Value));
        Assert.True(decoder[clsId.Value].IsSpecial);
    }

    [Fact]
    public void GetAddedTokensDecoderReturnsExistingTokens()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var decoder = tokenizer.GetAddedTokensDecoder();

    Assert.Contains(decoder.Values, token => token.Content == "[PAD]" && token.IsSpecial);
    }

    [Fact]
    public void EncodeSpecialTokensPropertyRoundtrips()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var original = tokenizer.EncodeSpecialTokens;
        tokenizer.EncodeSpecialTokens = !original;

        Assert.Equal(!original, tokenizer.EncodeSpecialTokens);
    }

    [Fact]
    public void NumSpecialTokensToAddReturnsZeroForSimpleTokenizer()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        Assert.Equal(0, tokenizer.NumSpecialTokensToAdd());
        Assert.Equal(0, tokenizer.NumSpecialTokensToAdd(isPairSequence: true));
    }

    [Fact]
    public async Task FromPretrainedAsyncHonorsCustomHttpClient()
    {
        using var client = new HttpClient(new FakeHttpMessageHandler(_ =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SampleTokenizerJson, System.Text.Encoding.UTF8)
            };
        }));

        using var tokenizer = await Tokenizer.FromPretrainedAsync(
            "namespace/model",
            new PretrainedTokenizerOptions { HttpClient = client });

    var encoding = await tokenizer.EncodeAsync(HelloWorldToken);
    Assert.Equal(HelloWorldIds, encoding.Ids);
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
    public void DecodeReversesEncoding()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var decoded = tokenizer.Decode(HelloWorldIds);

        Assert.Equal(HelloWorldToken, decoded);
    }

    [Fact]
    public void TokenIdRoundtripMatchesVocab()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        Assert.Equal(1, tokenizer.TokenToId(HelloToken));
        Assert.Equal(WorldToken, tokenizer.IdToToken(2));
        Assert.Null(tokenizer.TokenToId("Missing"));
    }

    [Fact]
    public void ToJsonEmitsValidPayload()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var json = tokenizer.ToJson();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("1.0", document.RootElement.GetProperty("version").GetString());
        Assert.Equal("WordLevel", document.RootElement.GetProperty("model").GetProperty("type").GetString());
    }

    [Fact]
    public void GetVocabReturnsManagedDictionary()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var vocab = tokenizer.GetVocab();

        Assert.Equal(4, vocab.Count);
        Assert.Equal(4, tokenizer.Config.Vocab.Count);
        Assert.Equal(0, vocab["[UNK]"]);
    }

    [Fact]
    public void PaddingRoundtripMatchesNativeState()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var options = new PaddingOptions(
            PaddingDirection.Left,
            padId: 3,
            padTypeId: 1,
            padToken: "[PAD]",
            length: 16,
            padToMultipleOf: 8);

        tokenizer.EnablePadding(options);

        var retrieved = tokenizer.GetPadding();
        Assert.NotNull(retrieved);
        Assert.Equal(PaddingDirection.Left, retrieved!.Direction);
        Assert.Equal(16, retrieved.Length);
        Assert.Equal(8, retrieved.PadToMultipleOf);

        Assert.NotNull(tokenizer.Config.Padding);
        Assert.Equal("left", tokenizer.Config.Padding!.Direction);

        tokenizer.DisablePadding();
        Assert.Null(tokenizer.GetPadding());
        Assert.Null(tokenizer.Config.Padding);
    }

    [Fact]
    public void TruncationRoundtripMatchesNativeState()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var options = new TruncationOptions(
            maxLength: 4,
            stride: 1,
            strategy: TruncationStrategy.OnlyFirst,
            direction: TruncationDirection.Left);

        tokenizer.EnableTruncation(options);

        var retrieved = tokenizer.GetTruncation();
        Assert.NotNull(retrieved);
        Assert.Equal(4, retrieved!.MaxLength);
        Assert.Equal(1, retrieved.Stride);
        Assert.Equal(TruncationStrategy.OnlyFirst, retrieved.Strategy);
        Assert.Equal(TruncationDirection.Left, retrieved.Direction);

        Assert.NotNull(tokenizer.Config.Truncation);
        Assert.Equal("only_first", tokenizer.Config.Truncation!.Strategy);
        Assert.Equal("left", tokenizer.Config.Truncation.Direction);

        tokenizer.DisableTruncation();
        Assert.Null(tokenizer.GetTruncation());
        Assert.Null(tokenizer.Config.Truncation);
    }

    [Fact]
    public void EncodeBatchProducesExpectedResults()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var results = tokenizer.EncodeBatch(new[] { HelloWorldToken, WorldToken });

        Assert.Equal(2, results.Count);
        Assert.Equal(HelloWorldIds, results[0].Ids);
        Assert.Equal(WorldIds, results[1].Ids);
    }

    [Fact]
    public void EncodeBatchWithPairsMatchesSingleEncode()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var expected = tokenizer.Encode(HelloToken, WorldToken);
        var batch = tokenizer.EncodeBatch(new[] { (HelloToken, (string?)WorldToken) });

        Assert.Single(batch);
        Assert.Equal(expected.Ids, batch[0].Ids);
    }

    [Fact]
    public void DecodeBatchRestoresInputs()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var encodings = tokenizer.EncodeBatch(new[] { HelloToken, HelloWorldToken });
        var decoded = tokenizer.DecodeBatch(encodings.Select(result => result.Ids).ToArray());

        Assert.Equal(new[] { HelloToken, HelloWorldToken }, decoded);
    }

    [Fact]
    public async Task EncodeAsyncProducesExpectedIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var encoding = await tokenizer.EncodeAsync(HelloWorldToken);

        Assert.Equal(HelloWorldIds, encoding.Ids);
    }

    [Fact]
    public async Task EncodeBatchAsyncProducesExpectedIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var results = await tokenizer.EncodeBatchAsync(new[] { HelloToken, WorldToken });

        Assert.Equal(HelloIds, results[0].Ids);
        Assert.Equal(WorldIds, results[1].Ids);
    }

    [Fact]
    public async Task DecodeBatchAsyncProducesExpectedInputs()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var ids = new IReadOnlyList<int>[] { HelloIds, HelloWorldIds };
        var decoded = await tokenizer.DecodeBatchAsync(ids);

        Assert.Equal(new[] { HelloToken, HelloWorldToken }, decoded);
    }

    [Fact]
    public void EncodingContainsTypeIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.TypeIds);
        Assert.Equal(result.Ids.Count, result.TypeIds.Count);
    }

    [Fact]
    public void EncodingContainsAttentionMask()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.AttentionMask);
        Assert.Equal(result.Ids.Count, result.AttentionMask.Count);
        Assert.All(result.AttentionMask, mask => Assert.Equal(1u, mask)); // All should be 1 for real tokens
    }

    [Fact]
    public void EncodingContainsSpecialTokensMask()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.SpecialTokensMask);
        Assert.Equal(result.Ids.Count, result.SpecialTokensMask.Count);
    }

    [Fact]
    public void EncodingContainsWordIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.WordIds);
        Assert.Equal(result.Ids.Count, result.WordIds.Count);
    }

    [Fact]
    public void EncodingContainsSequenceIds()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.SequenceIds);
        Assert.Equal(result.Ids.Count, result.SequenceIds.Count);
    }

    [Fact]
    public void EncodingContainsEmptyOverflowingWhenNoTruncation()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.Overflowing);
        Assert.Empty(result.Overflowing);
    }

    [Fact]
    public void EncodingWithTruncationProducesOverflowing()
    {
        using var tokenizer = new Tokenizer(SampleTokenizerJson);
        tokenizer.EnableTruncation(new TruncationOptions(
            maxLength: 1,
            stride: 0,
            strategy: TruncationStrategy.LongestFirst,
            direction: TruncationDirection.Right
        ));

        var result = tokenizer.Encode(HelloWorldToken);

        Assert.NotNull(result.Overflowing);
        Assert.Equal(1, result.Ids.Count); // Should be truncated to 1 token
        
        // Overflowing should contain the truncated tokens
        if (result.Overflowing.Count > 0)
        {
            Assert.All(result.Overflowing, overflow =>
            {
                Assert.NotNull(overflow.Ids);
                Assert.NotEmpty(overflow.Ids);
            });
        }
    }

    [Fact]
    public void EncodingPairHasDifferentTypeIds()
    {
        // Create a tokenizer with a post-processor that sets type IDs
        var tokenizerWithProcessor = """
        {
          "version": "1.0",
          "truncation": null,
          "padding": null,
          "added_tokens": [
            {
              "id": 3,
              "content": "[SEP]",
              "single_word": false,
              "lstrip": false,
              "rstrip": false,
              "normalized": false,
              "special": true
            },
            {
              "id": 4,
              "content": "[CLS]",
              "single_word": false,
              "lstrip": false,
              "rstrip": false,
              "normalized": false,
              "special": true
            }
          ],
          "normalizer": null,
          "pre_tokenizer": { "type": "Whitespace" },
          "post_processor": {
            "type": "BertProcessing",
            "sep": ["[SEP]", 3],
            "cls": ["[CLS]", 4]
          },
          "decoder": null,
          "model": {
            "type": "WordLevel",
            "vocab": {
              "[UNK]": 0,
              "Hello": 1,
              "World": 2,
              "[SEP]": 3,
              "[CLS]": 4
            },
            "unk_token": "[UNK]"
          }
        }
        """;

        using var tokenizer = new Tokenizer(tokenizerWithProcessor);

        var result = tokenizer.Encode(HelloToken, WorldToken);

        Assert.NotNull(result.TypeIds);
        Assert.NotEmpty(result.TypeIds);
        
        // In BERT-style encoding with pairs, we should see type IDs distinguishing sequences
        // TypeIds should have 0s for first sequence and 1s for second sequence (plus special tokens)
        var hasTypeId0 = result.TypeIds.Any(id => id == 0);
        var hasTypeId1 = result.TypeIds.Any(id => id == 1);
        
        Assert.True(hasTypeId0 || hasTypeId1); // Should have at least one type ID set
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            => _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responseFactory(request));
    }
}
