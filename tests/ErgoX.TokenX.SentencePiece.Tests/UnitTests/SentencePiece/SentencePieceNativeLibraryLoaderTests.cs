namespace ErgoX.TokenX.SentencePiece.Tests.Unit;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ErgoX.TokenX.SentencePiece;
using ErgoX.TokenX.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class SentencePieceNativeLibraryLoaderTests
{
    private const string LoaderTypeName = "ErgoX.TokenX.SentencePiece.Internal.Interop.SentencePieceNativeLibraryLoader";

    private static readonly Assembly LoaderAssembly = typeof(SentencePieceEnvironment).Assembly;

    private static readonly Type LoaderType = LoaderAssembly.GetType(LoaderTypeName, throwOnError: true)!;

    private static readonly string EnvironmentVariableName = (string)LoaderType
        .GetField("EnvironmentVariable", BindingFlags.Static | BindingFlags.NonPublic)!
        .GetRawConstantValue()!;

    private static readonly string LibraryName = (string)LoaderAssembly
        .GetType("ErgoX.TokenX.SentencePiece.Internal.Interop.NativeMethods", throwOnError: true)!
        .GetField("LibraryName", BindingFlags.Static | BindingFlags.NonPublic)!
        .GetRawConstantValue()!;

    [Fact]
    public void Resolve_ReturnsZeroForUnknownLibrary()
    {
        var resolveMethod = LoaderType.GetMethod("Resolve", BindingFlags.Static | BindingFlags.NonPublic)!;
        var result = (IntPtr)resolveMethod.Invoke(null, new object?[] { "not_sentencepiece", LoaderAssembly, null })!;
        Assert.Equal(IntPtr.Zero, result);
    }

    [Fact]
    public void EnumerateCandidatePaths_IncludesEnvironmentVariableLibrary()
    {
        var candidateMethod = LoaderType.GetMethod("EnumerateCandidatePaths", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
    var expectedLibrary = Path.Combine(tempDirectory.Path, LibraryName + ".dll");
        File.WriteAllBytes(expectedLibrary, Array.Empty<byte>());

        var original = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        Environment.SetEnvironmentVariable(EnvironmentVariableName, tempDirectory.Path);

        try
        {
            var candidates = InvokeEnumerable(candidateMethod, LoaderAssembly).ToList();
            Assert.Contains(expectedLibrary, candidates, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableName, original);
        }
    }

    [Fact]
    public void EnumerateCandidatePaths_DeduplicatesRepeatedEnvironmentEntries()
    {
        var candidateMethod = LoaderType.GetMethod("EnumerateCandidatePaths", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
    var expectedLibrary = Path.Combine(tempDirectory.Path, LibraryName + ".dll");
        File.WriteAllBytes(expectedLibrary, Array.Empty<byte>());

    var duplicateValue = string.Join(Path.PathSeparator, tempDirectory.Path, tempDirectory.Path);
        var original = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        Environment.SetEnvironmentVariable(EnvironmentVariableName, duplicateValue);

        try
        {
            var matches = InvokeEnumerable(candidateMethod, LoaderAssembly)
                .Where(path => string.Equals(path, expectedLibrary, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.Single(matches);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableName, original);
        }
    }

    [Fact]
    public void EnumerateProbeDirectories_EmitsRuntimeSpecificSubdirectories()
    {
        var probeMethod = LoaderType.GetMethod("EnumerateProbeDirectories", BindingFlags.Static | BindingFlags.NonPublic)!;
        var directories = InvokeEnumerable(probeMethod, LoaderAssembly).ToList();

        var baseDirectory = AppContext.BaseDirectory;
        var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;

        if (!string.IsNullOrEmpty(baseDirectory) && !string.IsNullOrEmpty(runtimeIdentifier))
        {
            var exactRuntimePath = Path.Combine(baseDirectory, "runtimes", runtimeIdentifier, "native");
            Assert.Contains(exactRuntimePath, directories, StringComparer.OrdinalIgnoreCase);

            var separatorIndex = runtimeIdentifier.IndexOf('-');
            if (separatorIndex > 0)
            {
                var platform = runtimeIdentifier[..separatorIndex];
                var platformPath = Path.Combine(baseDirectory, "runtimes", platform, "native");
                Assert.Contains(platformPath, directories, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void EnumerateLibraryFiles_ReturnsWindowsSharedLibrary()
    {
        var enumerateMethod = LoaderType.GetMethod("EnumerateLibraryFiles", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
    var expectedLibrary = Path.Combine(tempDirectory.Path, LibraryName + ".dll");
        File.WriteAllBytes(expectedLibrary, Array.Empty<byte>());

        var candidates = InvokeEnumerable(enumerateMethod, tempDirectory.Path).ToList();
        Assert.Contains(expectedLibrary, candidates, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFilesForPattern_SupportsWildcards()
    {
        var enumeratePatternMethod = LoaderType.GetMethod("EnumerateFilesForPattern", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
    var expectedLibrary = Path.Combine(tempDirectory.Path, LibraryName + ".dll");
        File.WriteAllBytes(expectedLibrary, Array.Empty<byte>());

        var matches = InvokeEnumerable(enumeratePatternMethod, tempDirectory.Path, "*.dll").ToList();
        Assert.Contains(expectedLibrary, matches, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFilesForPattern_ReturnsExactMatchWithoutWildcards()
    {
        var enumeratePatternMethod = LoaderType.GetMethod("EnumerateFilesForPattern", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
        var expectedLibrary = Path.Combine(tempDirectory.Path, "exact.match");
        File.WriteAllBytes(expectedLibrary, Array.Empty<byte>());

        var matches = InvokeEnumerable(enumeratePatternMethod, tempDirectory.Path, "exact.match").ToList();
        Assert.Single(matches);
        Assert.Equal(expectedLibrary, matches[0], StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateFilesForPattern_IgnoresEmptyPatterns()
    {
        var enumeratePatternMethod = LoaderType.GetMethod("EnumerateFilesForPattern", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
        var matches = InvokeEnumerable(enumeratePatternMethod, tempDirectory.Path, string.Empty).ToList();
        Assert.Empty(matches);
    }

    [Fact]
    public void EnumerateFiles_ReturnsMatchesAcrossPatterns()
    {
        var enumerateMethod = LoaderType.GetMethod("EnumerateFiles", BindingFlags.Static | BindingFlags.NonPublic)!;

        using var tempDirectory = TemporaryDirectory.Create();
        var dllLibrary = Path.Combine(tempDirectory.Path, LibraryName + ".dll");
        var txtLibrary = Path.Combine(tempDirectory.Path, "lib" + LibraryName + ".txt");
        File.WriteAllBytes(dllLibrary, Array.Empty<byte>());
        File.WriteAllBytes(txtLibrary, Array.Empty<byte>());

        var matches = InvokeEnumerable(enumerateMethod, tempDirectory.Path, new[] { "*.dll", "*.txt" }).ToList();
        Assert.Contains(dllLibrary, matches, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(txtLibrary, matches, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnumerateLibraryFiles_HandlesMissingDirectories()
    {
        var enumerateMethod = LoaderType.GetMethod("EnumerateLibraryFiles", BindingFlags.Static | BindingFlags.NonPublic)!;
        var missingDirectory = Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid().ToString("N"));

        var matches = InvokeEnumerable(enumerateMethod, missingDirectory).ToList();
        Assert.Empty(matches);
    }

    private static IEnumerable<string> InvokeEnumerable(MethodInfo method, params object?[] arguments)
    {
        return ((IEnumerable?)method.Invoke(null, arguments) ?? Array.Empty<string>()).Cast<string>();
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        internal string Path { get; }

        internal static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sentencepiece-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch (IOException)
            {
                // Best effort cleanup; ignore IO errors because the directory is temporary.
            }
            catch (UnauthorizedAccessException)
            {
                // Best effort cleanup; ignore access errors during disposal.
            }
        }
    }
}

