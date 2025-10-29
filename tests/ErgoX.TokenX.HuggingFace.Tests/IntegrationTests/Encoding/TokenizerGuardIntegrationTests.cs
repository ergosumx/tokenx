namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Encoding;

using System;
using System.Collections.Generic;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class TokenizerGuardIntegrationTests : HuggingFaceTestBase, IDisposable
{
    private readonly Tokenizer tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));

    [Fact]
    public void FromFile_requires_path()
    {
        Assert.Throws<ArgumentException>(() => Tokenizer.FromFile(""));
    }

    [Fact]
    public void FromBuffer_requires_payload()
    {
        Assert.Throws<ArgumentException>(() => Tokenizer.FromBuffer(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public void Save_requires_target_path()
    {
        Assert.Throws<ArgumentException>(() => tokenizer.Save(""));
    }

    [Fact]
    public void EnablePadding_requires_options()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.EnablePadding(null!));
    }

    [Fact]
    public void EnableTruncation_requires_options()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.EnableTruncation(null!));
    }

    [Fact]
    public void Encode_requires_sequence()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.Encode(null!));
        Assert.Throws<ArgumentNullException>(() => tokenizer.Encode(null!, "pair"));
    }

    [Fact]
    public void EncodeBatch_validates_inputs()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.EncodeBatch((IEnumerable<string>)null!));
        Assert.Empty(tokenizer.EncodeBatch(Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => tokenizer.EncodeBatch(new string?[] { "hello", null! }!));

        Assert.Throws<ArgumentNullException>(() => tokenizer.EncodeBatch((IEnumerable<(string, string?)>)null!));
        Assert.Empty(tokenizer.EncodeBatch(Array.Empty<(string, string?)>()));
        Assert.Throws<ArgumentException>(() => tokenizer.EncodeBatch(new (string First, string? Second)[] { ("hi", null), (null!, "pair") }));
    }

    [Fact]
    public void Decode_validates_inputs()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.Decode(null!));
        Assert.Equal(string.Empty, tokenizer.Decode(Array.Empty<int>()));
    }

    [Fact]
    public void DecodeBatch_validates_inputs()
    {
        Assert.Throws<ArgumentNullException>(() => tokenizer.DecodeBatch(null!));
        Assert.Empty(tokenizer.DecodeBatch(Array.Empty<IReadOnlyList<int>>()));
        Assert.Throws<ArgumentException>(() => tokenizer.DecodeBatch(new IReadOnlyList<int>?[] { new List<int> { 1 }, null! }!));

        var allEmpty = tokenizer.DecodeBatch(new[]
        {
            (IReadOnlyList<int>)Array.Empty<int>(),
            (IReadOnlyList<int>)Array.Empty<int>()
        });

        Assert.All(allEmpty, value => Assert.Equal(string.Empty, value));
    }

    [Fact]
    public void DisablePadding_and_truncation_reset_state()
    {
        tokenizer.DisablePadding();
        Assert.Null(tokenizer.GetPadding());

        tokenizer.DisableTruncation();
        Assert.Null(tokenizer.GetTruncation());
    }

    public void Dispose()
    {
        tokenizer.Dispose();
    }
}

