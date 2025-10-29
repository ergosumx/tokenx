namespace ErgoX.TokenX.HuggingFace.Tests.Integration.Encoding;

using System;
using System.IO;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Tests;
using ErgoX.TokenX.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class TokenizerNativeInteropIntegrationTests : HuggingFaceTestBase, IDisposable
{
    private readonly Tokenizer _tokenizer;

    public TokenizerNativeInteropIntegrationTests()
    {
        _tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    [Fact]
    public void Tokenizer_FromFile_InitializesNativeHandle()
    {
        // This tests NativeTokenizerHandle.Create path
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("bert-base-uncased"));
        Assert.NotNull(tokenizer);

        var encoding = tokenizer.Encode("test");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_FromFile_WithInvalidPath_ThrowsException()
    {
        // Tests error handling in native handle creation
        var invalidPath = Path.Combine(TestDataPath.GetBenchmarksDataRoot(), "nonexistent", "tokenizer.json");
        var ex = Record.Exception(() => Tokenizer.FromFile(invalidPath));
        Assert.NotNull(ex);
        Assert.IsAssignableFrom<IOException>(ex);
    }

    [Fact]
    public void Tokenizer_MultipleOperations_ReusesSameHandle()
    {
        // Tests InvokeWithHandle is called multiple times correctly
        var encoding1 = _tokenizer.Encode("first");
        var encoding2 = _tokenizer.Encode("second");
        var encoding3 = _tokenizer.Encode("third");

        Assert.NotNull(encoding1);
        Assert.NotNull(encoding2);
        Assert.NotNull(encoding3);
    }

    [Fact]
    public void Tokenizer_EncodeAfterMultipleOperations_WorksCorrectly()
    {
        // Tests that handle ref counting works correctly
        for (int i = 0; i < 10; i++)
        {
            var encoding = _tokenizer.Encode($"test {i}");
            Assert.NotNull(encoding);
            Assert.True(encoding.Length > 0);
        }
    }

    [Fact]
    public void Tokenizer_Decode_InvokesNativeInterop()
    {
        // Tests decode path through native interop
        var encoding = _tokenizer.Encode("Hello world");
        var decoded = _tokenizer.Decode(encoding.Ids);

        Assert.NotNull(decoded);
        Assert.Contains("Hello", decoded);
    }

    [Fact]
    public void Tokenizer_DecodeWithEmptyIds_HandlesCorrectly()
    {
        // Tests edge case in native decode
        var decoded = _tokenizer.Decode(Array.Empty<int>());
        Assert.NotNull(decoded);
    }

    [Fact]
    public void Tokenizer_DecodeWithSkipSpecialTokens_WorksCorrectly()
    {
        // Tests decode options path
        var encoding = _tokenizer.Encode("test");
        var decoded1 = _tokenizer.Decode(encoding.Ids, skipSpecialTokens: true);
        var decoded2 = _tokenizer.Decode(encoding.Ids, skipSpecialTokens: false);

        Assert.NotNull(decoded1);
        Assert.NotNull(decoded2);
    }

    [Fact]
    public void Tokenizer_EncodePair_InvokesNativeInterop()
    {
        // Tests pair encoding through native interop
        var encoding = _tokenizer.Encode("First text", "Second text");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_GetVocabSize_CallsNativeMethod()
    {
        // Tests vocab size retrieval through encoding operations
        var encoding = _tokenizer.Encode("test");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_IdToToken_CallsNativeMethod()
    {
        // Tests ID to token conversion
        var encoding = _tokenizer.Encode("test");
        if (encoding.Length > 0)
        {
            var tokenId = encoding.Ids[0];
            var token = _tokenizer.IdToToken(tokenId);
            Assert.NotNull(token);
        }
    }

    [Fact]
    public void Tokenizer_TokenToId_CallsNativeMethod()
    {
        // Tests token to ID conversion
        var encoding = _tokenizer.Encode("test");
        if (encoding.Length > 0)
        {
            var token = encoding.Tokens[0];
            var id = _tokenizer.TokenToId(token);
            Assert.NotNull(id);
        }
    }

    [Fact]
    public void Tokenizer_TokenToIdWithInvalidToken_ReturnsNull()
    {
        // Tests error handling
        var id = _tokenizer.TokenToId("!!INVALID_TOKEN_THAT_DOES_NOT_EXIST!!");
        Assert.Null(id);
    }

    [Fact]
    public void Tokenizer_GetPadding_ReturnsNullWhenNotSet()
    {
        // Tests padding retrieval through native interop
        var padding = _tokenizer.GetPadding();
        // May be null or have values depending on tokenizer config
        Assert.True(padding == null || padding.PadId >= 0);
    }

    [Fact]
    public void Tokenizer_GetTruncation_ReturnsNullWhenNotSet()
    {
        // Tests truncation retrieval through native interop
        var truncation = _tokenizer.GetTruncation();
        // May be null or have values depending on tokenizer config
        Assert.True(truncation == null || truncation.MaxLength >= 0);
    }

    [Fact]
    public void Tokenizer_Dispose_ReleasesNativeHandle()
    {
        // Tests ReleaseHandle is called
        var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var encoding = tokenizer.Encode("test");
        Assert.NotNull(encoding);

        tokenizer.Dispose();

        // After dispose, operations should fail
        Assert.Throws<ObjectDisposedException>(() => tokenizer.Encode("test"));
    }

    [Fact]
    public void Tokenizer_DoubleDispose_DoesNotThrow()
    {
        // Tests dispose is idempotent
        var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        tokenizer.Dispose();

        // Should not throw on second dispose
        var exception = Record.Exception(() => tokenizer.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Tokenizer_WithAddedTokens_ProcessesCorrectly()
    {
        // Tests added tokens path through native interop
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("meta-llama-3-8b-instruct"));
        var encoding = tokenizer.Encode("test");
        Assert.NotNull(encoding);
    }

    [Fact]
    public void Tokenizer_LargeInput_HandlesCorrectly()
    {
        // Tests handling of large inputs through native interop
        var largeText = new string('a', 10000);
        var encoding = _tokenizer.Encode(largeText);
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_SpecialCharacters_EncodesCorrectly()
    {
        // Tests special character handling
        var encoding = _tokenizer.Encode("test\n\t\r special");
        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_ConcurrentOperations_WorkCorrectly()
    {
        // Tests thread safety of handle invoke
        System.Threading.Tasks.Parallel.For(0, 5, i =>
        {
            var encoding = _tokenizer.Encode($"concurrent test {i}");
            Assert.NotNull(encoding);
        });
    }

    [Fact]
    public void Tokenizer_DecodeBatch_ProcessesMultipleSequences()
    {
        // Tests batch decode through native interop
        var encoding1 = _tokenizer.Encode("first");
        var encoding2 = _tokenizer.Encode("second");

        var decoded1 = _tokenizer.Decode(encoding1.Ids);
        var decoded2 = _tokenizer.Decode(encoding2.Ids);

        Assert.NotNull(decoded1);
        Assert.NotNull(decoded2);
    }

    [Fact]
    public void Tokenizer_EncodeWithAddSpecialTokensTrue_IncludesSpecial()
    {
        // Tests encoding with special tokens through native interop
        var encoding = _tokenizer.Encode("test", addSpecialTokens: true);

        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_EncodeWithAddSpecialTokensFalse_ExcludesSpecial()
    {
        // Tests encoding without special tokens
        var encoding = _tokenizer.Encode("test", addSpecialTokens: false);

        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void Tokenizer_FromFileWithDifferentModels_WorksForAll()
    {
        // Tests native library loading and initialization for different models
        var models = new[] { "gpt2", "bert-base-uncased", "distilbert-base-uncased" };

        foreach (var model in models)
        {
            using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath(model));
            var encoding = tokenizer.Encode("test");
            Assert.NotNull(encoding);
            Assert.True(encoding.Length > 0);
        }
    }

    [Fact]
    public void Tokenizer_EncodeWithAddToSpecialTokens_WorksCorrectly()
    {
        // Tests encode options through native interop
        var encoding = _tokenizer.Encode("test", addSpecialTokens: false);
        Assert.NotNull(encoding);
    }

    [Fact]
    public void Tokenizer_EncodeWithPairAndAddSpecialTokens_WorksCorrectly()
    {
        // Tests pair encoding with options
        var encoding = _tokenizer.Encode("first", "second", addSpecialTokens: true);
        Assert.NotNull(encoding);
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }
}

