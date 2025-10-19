namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.Internal;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;
using Xunit;

/// <summary>
/// Tests for native interop infrastructure including platform detection,
/// native library loading, and SafeHandle lifecycle patterns through public APIs.
/// </summary>
public class NativeInteropInfrastructureTests : IDisposable
{
    private readonly Tokenizer _tokenizer;

    public NativeInteropInfrastructureTests()
    {
        _tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tokenizer?.Dispose();
        }
    }

    #region Platform Detection Tests

    [Fact]
    public void RuntimeIdentifier_OnWindows_ReturnsWindowsRid()
    {
        // Tests platform detection logic
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            Assert.True(
                arch is Architecture.X64 or Architecture.X86 or Architecture.Arm64,
                "Unexpected Windows architecture");

            // Verify platform detection by loading a tokenizer (exercises NativeLibraryLoader)
            using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
            Assert.NotNull(tokenizer);
        }
    }

    [Fact]
    public void RuntimeIdentifier_Architecture_MatchesProcess()
    {
        // Verifies architecture detection
        var currentArch = RuntimeInformation.ProcessArchitecture;
        Assert.True(
            currentArch is Architecture.X64 or Architecture.X86 or Architecture.Arm64,
            $"Unexpected architecture: {currentArch}");
    }

    [Fact]
    public void NativeLibraryLoader_LoadsSuccessfully()
    {
        // Tests that ModuleInitializer ran and library loaded
        // By successfully creating a tokenizer, we verify the DLL was found and loaded
        using var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        var encoding = tokenizer.Encode("test");

        Assert.NotNull(encoding);
        Assert.True(encoding.Length > 0);
    }

    [Fact]
    public void NativeLibraryLoader_HandlesMultipleTokenizers()
    {
        // Tests that library resolution works for multiple instances
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

    #endregion

    #region SafeHandle Lifecycle Tests

    [Fact]
    public void SafeHandle_IsInvalid_WhenNotInitialized()
    {
        // Tests IsInvalid property through tokenizer disposal
        var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        Assert.NotNull(tokenizer);

        // Tokenizer is valid before disposal
        var encoding = tokenizer.Encode("test");
        Assert.NotNull(encoding);

        tokenizer.Dispose();

        // After disposal, operations should fail
        var ex = Record.Exception(() => tokenizer.Encode("test"));
        Assert.NotNull(ex);
    }

    [Fact]
    public void SafeHandle_DoubleDispose_IsSafe()
    {
        // Tests that SafeHandle can be disposed multiple times
        var tokenizer = Tokenizer.FromFile(TestDataPath.GetModelTokenizerPath("gpt2"));
        tokenizer.Dispose();

        var ex = Record.Exception(() => tokenizer.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public async Task SafeHandle_MultipleOperations_MaintainRefCount()
    {
        // Tests that multiple concurrent operations maintain proper ref count
        var tasks = new Task<EncodingResult>[10];

        for (int i = 0; i < 10; i++)
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

    #endregion

    #region Error Path Tests

    [Fact]
    public void NativeInterop_InvalidJson_ThrowsInvalidOperationException()
    {
        // Tests error path in native handle creation
        var invalidJson = "{ invalid json syntax }";
        var ex = Record.Exception(() => new Tokenizer(invalidJson));

        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void NativeInterop_EmptyJson_ThrowsArgumentException()
    {
        // Tests validation in native handle creation
        Assert.Throws<ArgumentException>(() => new Tokenizer(string.Empty));
        Assert.Throws<ArgumentException>(() => new Tokenizer("   "));
    }

    [Fact]
    public void NativeInterop_MalformedTokenizerJson_Throws()
    {
        // Tests native error handling
        const string malformedJson = @"{""version"": ""1.0"", ""truncation"": null, ""padding"": null}";
        var ex = Record.Exception(() => new Tokenizer(malformedJson));

        Assert.NotNull(ex);
    }

    [Fact]
    public void NativeInterop_GetLastErrorMessage_ReturnsErrorDetails()
    {
        // Tests error message retrieval
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            using var _ = new Tokenizer("{ invalid }");
        });

        // Error message should contain details from native layer
        Assert.NotNull(ex.Message);
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void NativeInterop_FromPretrained_WithInvalidIdentifier_Throws()
    {
        // Tests error handling in pretrained loading
        var ex = Record.Exception(() => Tokenizer.FromPretrained(string.Empty));
        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void NativeInterop_FromPretrained_WithNonexistentModel_Throws()
    {
        // Tests network/file error handling
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
        // Tests error handling under concurrent load
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

    #endregion
}
