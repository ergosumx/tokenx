namespace Tokenizers.Benchmarks.Assets;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Tokenizers.Benchmarks.Infrastructure;

internal sealed record TokenizerModelAssets(
    string Name,
    string TokenizerJsonPath,
    string? VocabPath = null,
    string? MergesPath = null,
    string? SentencePieceModelPath = null);

internal sealed record RemoteAsset(string FileName, string Uri, string? Sha256 = null);

internal static class ModelAssets
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static TokenizerModelAssets EnsureGpt2()
    {
        var files = EnsureFiles(
            "gpt2",
            new RemoteAsset("tokenizer.json", "https://huggingface.co/gpt2/raw/main/tokenizer.json"),
            new RemoteAsset("vocab.json", "https://huggingface.co/gpt2/raw/main/vocab.json"),
            new RemoteAsset("merges.txt", "https://huggingface.co/gpt2/raw/main/merges.txt"));

        return new TokenizerModelAssets(
            "GPT-2",
            files["tokenizer.json"],
            VocabPath: files["vocab.json"],
            MergesPath: files["merges.txt"]);
    }

    public static TokenizerModelAssets EnsureBertBaseUncased()
    {
        var files = EnsureFiles(
            "bert-base-uncased",
            new RemoteAsset("tokenizer.json", "https://huggingface.co/bert-base-uncased/raw/main/tokenizer.json"),
            new RemoteAsset("vocab.txt", "https://huggingface.co/bert-base-uncased/raw/main/vocab.txt"));

        return new TokenizerModelAssets(
            "BERT Base Uncased",
            files["tokenizer.json"],
            VocabPath: files["vocab.txt"]);
    }

    public static TokenizerModelAssets EnsureT5Small()
    {
        var files = EnsureFiles(
            "t5-small",
            new RemoteAsset("tokenizer.json", "https://huggingface.co/t5-small/raw/main/tokenizer.json"),
            new RemoteAsset("spiece.model", "https://huggingface.co/t5-small/raw/main/spiece.model"));

        return new TokenizerModelAssets(
            "T5 Small",
            files["tokenizer.json"],
            SentencePieceModelPath: files["spiece.model"]);
    }

    private static Dictionary<string, string> EnsureFiles(string modelFolder, params RemoteAsset[] assets)
    {
        var root = Path.Combine(PathUtilities.GetBenchmarkAssetsRoot(), modelFolder);
        Directory.CreateDirectory(root);

        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in assets)
        {
            var destination = Path.Combine(root, asset.FileName);
            if (!File.Exists(destination))
            {
                DownloadAssetAsync(asset, destination).GetAwaiter().GetResult();
            }

            resolved[asset.FileName] = destination;
        }

        return resolved;
    }

    private static async Task DownloadAssetAsync(RemoteAsset asset, string destination)
    {
        using var response = await HttpClient.GetAsync(asset.Uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to download {asset.Uri} ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }

        await using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var destinationStream = File.Create(destination);
        await source.CopyToAsync(destinationStream).ConfigureAwait(false);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VecraX-Benchmarks/1.0");
        return client;
    }
}
