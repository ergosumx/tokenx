namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

public sealed class SentencePieceModelFixture : IAsyncLifetime
{
    private static readonly string Mt5ModelPath = RepositoryTestData.GetPath("google-mt5-small", "spiece.model");
    private static readonly string T5ModelPath = RepositoryTestData.GetPath("t5-small", "spiece.model");
    private static readonly string LlamaModelPath = RepositoryTestData.GetPath("openchat-3.5-1210", "tokenizer.model");

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
            throw new FileNotFoundException($"SentencePiece model asset not found at '{absolutePath}'. Ensure tests/_TestData has been restored.", absolutePath);
        }

        return await File.ReadAllBytesAsync(absolutePath).ConfigureAwait(false);
    }
}
