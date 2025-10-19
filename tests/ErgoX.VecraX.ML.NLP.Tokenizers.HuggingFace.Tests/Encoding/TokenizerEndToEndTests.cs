namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Encoding;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
public sealed class TokenizerEndToEndTests : IDisposable
{
    private readonly Tokenizer tokenizer;

    public TokenizerEndToEndTests()
    {
        var tokenizerPath = TestDataPath.GetModelTokenizerPath("meta-llama-3-8b-instruct");
        tokenizer = Tokenizer.FromFile(tokenizerPath);
    }

    [Fact]
    public void Encode_and_decode_round_trip_sequences()
    {
        var encoding = tokenizer.Encode("Hello world");
        Assert.NotEmpty(encoding.Ids);
        Assert.NotEmpty(encoding.Tokens);

        var decoded = tokenizer.Decode(encoding.Ids);
        Assert.Contains("Hello", decoded, StringComparison.Ordinal);

        var pairEncoding = tokenizer.Encode("Hello", "world");
        Assert.NotEmpty(pairEncoding.Ids);

        var batch = tokenizer.EncodeBatch(new[] { "Quick brown fox", "jumps over" });
        Assert.Equal(2, batch.Count);

        var tupleBatch = tokenizer.EncodeBatch(new (string First, string? Second)[]
        {
            ("Hello", "world"),
            ("VecraX", null)
        });

        Assert.Equal(2, tupleBatch.Count);

    var decodedBatch = tokenizer.DecodeBatch(batch.Select(result => result.Ids).ToArray());
        Assert.Equal(batch.Count, decodedBatch.Count);
        Assert.All(decodedBatch, text => Assert.False(string.IsNullOrWhiteSpace(text)));

    var decodedPairBatch = tokenizer.DecodeBatch(tupleBatch.Select(result => result.Ids).ToArray(), skipSpecialTokens: false);
        Assert.Equal(tupleBatch.Count, decodedPairBatch.Count);

        var emptyBatch = tokenizer.EncodeBatch(Array.Empty<string>());
        Assert.Empty(emptyBatch);

        var decodedEmpty = tokenizer.DecodeBatch(new List<IReadOnlyList<int>> { Array.Empty<int>() });
        Assert.Single(decodedEmpty);
        Assert.Equal(string.Empty, decodedEmpty[0]);

        Assert.Equal(string.Empty, tokenizer.Decode(Array.Empty<int>()));

        var firstToken = encoding.Tokens[0];
        var firstTokenId = tokenizer.TokenToId(firstToken);
        Assert.True(firstTokenId.HasValue);
        Assert.Equal(encoding.Ids[0], firstTokenId.Value);
        Assert.Equal(firstToken, tokenizer.IdToToken((uint)firstTokenId.Value));
        Assert.Null(tokenizer.TokenToId(string.Empty));
        Assert.Null(tokenizer.IdToToken(-1));
    }

    [Fact]
    public void Configure_padding_and_truncation_settings()
    {
        tokenizer.DisablePadding();
        Assert.Null(tokenizer.GetPadding());

        var paddingOptions = new PaddingOptions(PaddingDirection.Left, padId: 1, padTypeId: 2, padToken: "<pad>", length: 8, padToMultipleOf: 4);
        tokenizer.EnablePadding(paddingOptions);

        var padding = tokenizer.GetPadding();
        Assert.NotNull(padding);
        Assert.Equal(PaddingDirection.Left, padding!.Direction);
        Assert.Equal((uint)1, padding.PadId);
        Assert.Equal((uint)2, padding.PadTypeId);
        Assert.Equal("<pad>", padding.PadToken);
        Assert.Equal(8, padding.Length);
        Assert.Equal(4, padding.PadToMultipleOf);

        tokenizer.DisablePadding();
        Assert.Null(tokenizer.GetPadding());

        tokenizer.DisableTruncation();
        Assert.Null(tokenizer.GetTruncation());

        var truncationOptions = new TruncationOptions(maxLength: 16, stride: 2, TruncationStrategy.OnlyFirst, TruncationDirection.Left);
        tokenizer.EnableTruncation(truncationOptions);

        var truncation = tokenizer.GetTruncation();
        Assert.NotNull(truncation);
        Assert.Equal(16, truncation!.MaxLength);
        Assert.Equal(2, truncation.Stride);
        Assert.Equal(TruncationStrategy.OnlyFirst, truncation.Strategy);
        Assert.Equal(TruncationDirection.Left, truncation.Direction);

        tokenizer.DisableTruncation();
        Assert.Null(tokenizer.GetTruncation());
    }

    [Fact]
    public void Save_serializes_configuration_to_disk()
    {
        var json = tokenizer.ToJson(pretty: true);
        Assert.False(string.IsNullOrWhiteSpace(json));

        var tempPath = Path.Combine(Path.GetTempPath(), $"tokenizer-{Guid.NewGuid():N}.json");
        tokenizer.Save(tempPath);

        Assert.True(File.Exists(tempPath));
        Assert.True(new FileInfo(tempPath).Length > 0);

        File.Delete(tempPath);
    }

    [Fact]
    public void Encode_batch_validates_inputs()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.EncodeBatch((IEnumerable<string>)null!));
        Assert.Throws<ArgumentNullException>(() => tokenizer.EncodeBatch((IEnumerable<(string, string?)>)null!));

        var sequences = new List<string> { "valid", null! };
        Assert.Throws<ArgumentException>(() => tokenizer.EncodeBatch(sequences));

        var pairs = new List<(string First, string? Second)> { ("valid", null), (null!, null) };
        Assert.Throws<ArgumentException>(() => tokenizer.EncodeBatch(pairs));

        Assert.Throws<ArgumentNullException>(() => tokenizer.DecodeBatch(null!));

        var longList = new List<IReadOnlyList<int>>();
        for (var i = 0; i < 4; i++)
        {
            longList.Add(new List<int> { 0, 1, 2, 3, 4, 5 });
        }

        var decoded = tokenizer.DecodeBatch(longList, skipSpecialTokens: false);
        Assert.Equal(longList.Count, decoded.Count);
    }

    public void Dispose()
    {
        tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
}
