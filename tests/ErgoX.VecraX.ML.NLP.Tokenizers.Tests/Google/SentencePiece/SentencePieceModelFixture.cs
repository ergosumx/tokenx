namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Tests;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Processing;
using Xunit;

public sealed class SentencePieceModelFixture : IAsyncLifetime
{
    private static readonly Assembly ResourceAssembly = typeof(SentencePieceModelFixture).Assembly;
    private static readonly string DataDirectory = Path.Combine(AppContext.BaseDirectory, "TestData", "Google", "SentencePiece");

    public ReadOnlyMemory<byte> Mt5SmallModel { get; private set; }

    public ReadOnlyMemory<byte> T5SmallModel { get; private set; }

    public ReadOnlyMemory<byte> LlamaModel { get; private set; }

    public async Task InitializeAsync()
    {
        Mt5SmallModel = await LoadModelAsync("t5-efficient-tiny-spiece.model").ConfigureAwait(false);
        T5SmallModel = await LoadModelAsync("tiny-mbart-sentencepiece.bpe.model").ConfigureAwait(false);
        LlamaModel = await LoadModelAsync("llama-tokenizer.model").ConfigureAwait(false);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static SentencePieceProcessor CreateProcessor(ReadOnlyMemory<byte> model)
    {
        var processor = new SentencePieceProcessor();
        processor.Load(model.Span);
        return processor;
    }

    private static async Task<ReadOnlyMemory<byte>> LoadModelAsync(string fileName)
    {
        var path = Path.Combine(DataDirectory, fileName);
        if (File.Exists(path))
        {
            return await File.ReadAllBytesAsync(path).ConfigureAwait(false);
        }

        var resourceName = ResourceAssembly.GetManifestResourceNames();
        foreach (var candidate in resourceName)
        {
            if (!candidate.EndsWith(fileName, StringComparison.Ordinal))
            {
                continue;
            }

            using var stream = ResourceAssembly.GetManifestResourceStream(candidate) ?? throw new FileNotFoundException($"Embedded SentencePiece model '{fileName}' could not be opened.", fileName);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory).ConfigureAwait(false);
            var data = memory.ToArray();

            try
            {
                Directory.CreateDirectory(DataDirectory);
                await File.WriteAllBytesAsync(path, data).ConfigureAwait(false);
            }
            catch (Exception writeError) when (writeError is IOException or UnauthorizedAccessException)
            {
                GC.KeepAlive(writeError);
            }

            return data;
        }

        throw new FileNotFoundException($"SentencePiece model '{fileName}' was not found in the test assets.", path);
    }
}
