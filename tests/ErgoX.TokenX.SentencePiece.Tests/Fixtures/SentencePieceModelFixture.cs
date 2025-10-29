namespace ErgoX.TokenX.SentencePiece.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using ErgoX.TokenX.SentencePiece.Processing;
using Xunit;

public sealed class SentencePieceModelFixture : IAsyncLifetime
{
    private static readonly string Mt5ModelPath = SentencePieceTestDataPath.GetModelPath("google-mt5-small", "spiece.model");
    private static readonly string T5ModelPath = SentencePieceTestDataPath.GetModelPath("t5-small", "spiece.model");
    private static readonly string LlamaModelPath = SentencePieceTestDataPath.GetModelPath("openchat-3.5-1210", "tokenizer.model");

    public ReadOnlyMemory<byte> Mt5SmallModel { get; private set; }

    public ReadOnlyMemory<byte> T5SmallModel { get; private set; }

    public ReadOnlyMemory<byte> LlamaModel { get; private set; }

    public async Task InitializeAsync()
    {
        Mt5SmallModel = await LoadModelAsync(Mt5ModelPath).ConfigureAwait(false);
        T5SmallModel = await LoadModelAsync(T5ModelPath).ConfigureAwait(false);
        LlamaModel = await LoadModelAsync(LlamaModelPath).ConfigureAwait(false);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static SentencePieceProcessor CreateProcessor(ReadOnlyMemory<byte> model)
    {
        var processor = new SentencePieceProcessor();
        processor.Load(model.Span);
        return processor;
    }

    private static async Task<ReadOnlyMemory<byte>> LoadModelAsync(string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            throw new ArgumentException("Model asset path must be provided.", nameof(absolutePath));
        }

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"SentencePiece model asset not found at '{absolutePath}'. Ensure tests/_sentencepeice assets are available.", absolutePath);
        }

        return await File.ReadAllBytesAsync(absolutePath).ConfigureAwait(false);
    }
}

