namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Integration.Internal;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Filter, TestCategories.Integration)]
public sealed class NativeInteropInfrastructureIntegrationTests : HuggingFaceTestBase, IDisposable
{
    private readonly Tokenizer _tokenizer;

    public NativeInteropInfrastructureIntegrationTests()
    {
        _tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tokenizer.Dispose();
        }
    }

    [Fact]
    public void RuntimeIdentifier_OnWindows_ReturnsWindowsRid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            Assert.True(
                arch is Architecture.X64 or Architecture.X86 or Architecture.Arm64,
                "Unexpected Windows architecture");

            using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
            Assert.NotNull(tokenizer);
        }
    }

    [Fact]
    public void RuntimeIdentifier_Architecture_MatchesProcess()
    {
        var currentArch = RuntimeInformation.ProcessArchitecture;
        Assert.True(
            currentArch is Architecture.X64 or Architecture.X86 or Architecture.Arm64,
            $"Unexpected architecture: {currentArch}");
    }

    [Fact]
    public void NativeLibraryLoader_LoadsSuccessfully()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var encoding = tokenizer.Encode("test");

        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void NativeLibraryLoader_HandlesMultipleTokenizers()
    {
        using var tokenizer1 = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        using var tokenizer2 = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("bert-base-uncased"));
        using var tokenizer3 = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("distilbert-base-uncased"));

        var encoding1 = tokenizer1.Encode("test");
        var encoding2 = tokenizer2.Encode("test");
        var encoding3 = tokenizer3.Encode("test");

        Assert.NotNull(encoding1);
        Assert.NotNull(encoding2);
        Assert.NotNull(encoding3);
    }

    [Fact]
    public void SafeHandle_IsInvalid_WhenNotInitialized()
    {
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var encoding = tokenizer.Encode("test");
        Assert.NotNull(encoding);

        tokenizer.Dispose();

        var ex = Record.Exception(() => tokenizer.Encode("test"));
        Assert.NotNull(ex);
    }

    [Fact]
    public void SafeHandle_DoubleDispose_IsSafe()
    {
        var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        tokenizer.Dispose();

        var ex = Record.Exception(() => tokenizer.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public async Task SafeHandle_MultipleOperations_MaintainRefCount()
    {
        var tasks = new Task<EncodingResult>[10];

        for (var i = 0; i < tasks.Length; i++)
        {
            var index = i;
            tasks[i] = Task.Run(async () =>
            {
                var encoding = _tokenizer.Encode($"test {index}");
                await Task.Delay(Random.Shared.Next(1, 20)).ConfigureAwait(false);
                return encoding;
            });
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (var task in tasks)
        {
            Assert.NotNull(task.Result);
            Assert.True(task.Result.Length > 0);
        }
    }

    [Fact]
    public void NativeInterop_InvalidJson_ThrowsInvalidOperationException()
    {
        var invalidJson = "{ invalid json syntax }";
        var ex = Record.Exception(() => new Tokenizer(invalidJson));

        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void NativeInterop_EmptyJson_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Tokenizer(string.Empty));
        Assert.Throws<ArgumentException>(() => new Tokenizer("   "));
    }

    [Fact]
    public void NativeInterop_MalformedTokenizerJson_Throws()
    {
        const string malformedJson = "{\"version\": \"1.0\", \"truncation\": null, \"padding\": null}";
        var ex = Record.Exception(() => new Tokenizer(malformedJson));

        Assert.NotNull(ex);
    }

    [Fact]
    public void NativeInterop_GetLastErrorMessage_ReturnsErrorDetails()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            using var _ = new Tokenizer("{ invalid }");
        });

        Assert.NotNull(ex.Message);
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void NativeInterop_FromPretrained_WithInvalidIdentifier_Throws()
    {
        var ex = Record.Exception(() => Tokenizer.FromPretrained(string.Empty));
        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void NativeInterop_FromPretrained_WithNonexistentModel_Throws()
    {
        var ex = Record.Exception(() =>
        {
            using var _ = Tokenizer.FromPretrained("nonexistent/model/that/does/not/exist/12345");
        });

        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void NativeInterop_ConcurrentErrorScenarios_ThreadSafe()
    {
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<InvalidOperationException>();

        var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
        {
            try
            {
                using var _ = new Tokenizer($"{{ invalid json {i} }}");
            }
            catch (InvalidOperationException ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        Task.WaitAll(tasks);

        Assert.Equal(5, exceptions.Count);
        Assert.All(exceptions, ex => Assert.IsType<InvalidOperationException>(ex));
    }
}
