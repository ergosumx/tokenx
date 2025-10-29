namespace ErgoX.TokenX.Tests.IntegrationTests.Tiktoken;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErgoX.TokenX.Tests;
using ErgoX.TokenX.Tiktoken;
using Xunit;

public sealed class TiktokenIntegrationTests : TiktokenTestBase
{
    private const string Pattern = "(?s).";

    private static readonly IReadOnlyDictionary<string, int> SpecialTokens = new Dictionary<string, int>
    {
        ["<|special|>"] = 8,
    };

    private static readonly string BpeContent = string.Join(
        Environment.NewLine,
        "aA== 0",
        "aQ== 1",
        "IA== 2",
        "dw== 3",
        "bw== 4",
        "cg== 5",
        "bA== 6",
        "ZA== 7");

    [Fact]
    public void FromTiktokenStream_CanEncodeAndDecode()
    {
        using var stream = CreateBpeStream();

        using var encoding = TiktokenEncodingFactory.FromTiktokenStream(
            "integration",
            Pattern,
            stream,
            SpecialTokens,
            explicitVocabularySize: 9);

    var tokens = encoding.Encode("hi <|special|>", new[] { "<|special|>" });
        Assert.Equal(new uint[] { 0, 1, 2, 8 }, tokens);

    var tokenArray = tokens.ToArray();

    var text = encoding.Decode(tokenArray);
        Assert.Equal("hi <|special|>", text);

    var bytes = encoding.DecodeBytes(tokenArray);
        Assert.Equal(Encoding.UTF8.GetBytes("hi <|special|>"), bytes);
    }

    [Fact]
    public void FromTiktokenFile_LoadsFromDisk()
    {
        var path = WriteTemporaryBpeFile();
        try
        {
            using var encoding = TiktokenEncodingFactory.FromTiktokenFile(
                "integration",
                Pattern,
                path,
                SpecialTokens,
                explicitVocabularySize: 9);

            var tokens = encoding.EncodeOrdinary("hi ");

            Assert.Equal(new uint[] { 0, 1, 2 }, tokens);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FromTiktokenFile_ThrowsWhenMissing()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var ex = Assert.Throws<FileNotFoundException>(
            () => TiktokenEncodingFactory.FromTiktokenFile(
                "integration",
                Pattern,
                missing,
                SpecialTokens,
                explicitVocabularySize: 9));

        Assert.Contains(missing, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static MemoryStream CreateBpeStream()
    {
        var bytes = Encoding.ASCII.GetBytes(BpeContent + Environment.NewLine);
        return new MemoryStream(bytes, writable: false);
    }

    private static string WriteTemporaryBpeFile()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, BpeContent);
        return path;
    }
}

