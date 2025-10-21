namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.Common.Interop;

[ExcludeFromCodeCoverage] // Platform-dependent enumeration of native search paths is validated via higher-level tests.
internal static class SentencePieceNativeLibraryLoader
{
    private const string EnvironmentVariable = "SENTENCEPIECE_NATIVE_PATH";

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibraryResolverRegistry.Register(typeof(SentencePieceNativeLibraryLoader).Assembly, Resolve);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeMethods.LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        foreach (var candidate in EnumerateCandidatePaths(assembly))
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> EnumerateCandidatePaths(Assembly assembly)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in EnumerateProbeDirectories(assembly))
        {
            foreach (var file in EnumerateLibraryFiles(directory).Where(seen.Add))
            {
                yield return file;
            }
        }
    }

    private static IEnumerable<string> EnumerateProbeDirectories(Assembly assembly)
    {
        static IEnumerable<string> SplitPaths(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield break;
            }

            foreach (var part in value.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    yield return trimmed;
                }
            }
        }

        var baseDirectory = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDirectory))
        {
            yield return baseDirectory;
            foreach (var runtimeDirectory in EnumerateRuntimeSpecificDirectories(baseDirectory))
            {
                yield return runtimeDirectory;
            }
        }

        var assemblyDirectory = GetAssemblyDirectory(assembly);
        if (!string.IsNullOrEmpty(assemblyDirectory) && !string.Equals(assemblyDirectory, baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            yield return assemblyDirectory;
            foreach (var runtimeDirectory in EnumerateRuntimeSpecificDirectories(assemblyDirectory))
            {
                yield return runtimeDirectory;
            }
        }

        foreach (var path in SplitPaths(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            yield return path;
        }
    }

    private static string? GetAssemblyDirectory(Assembly assembly)
    {
        try
        {
            var location = assembly.Location;
            return string.IsNullOrEmpty(location) ? null : Path.GetDirectoryName(location);
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static IEnumerable<string> EnumerateRuntimeSpecificDirectories(string root)
    {
        if (string.IsNullOrEmpty(root))
        {
            yield break;
        }

        var rid = RuntimeInformation.RuntimeIdentifier;
        if (!string.IsNullOrEmpty(rid))
        {
            yield return Path.Combine(root, "runtimes", rid, "native");

            var separatorIndex = rid.IndexOf('-');
            if (separatorIndex > 0)
            {
                var platform = rid[..separatorIndex];
                yield return Path.Combine(root, "runtimes", platform, "native");
            }
        }
    }

    [ExcludeFromCodeCoverage] // Platform-dependent file patterns make it impractical to exercise all branches on a single OS.
    private static IEnumerable<string> EnumerateLibraryFiles(string directory)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            yield break;
        }

        if (OperatingSystem.IsWindows())
        {
            var file = Path.Combine(directory, NativeMethods.LibraryName + ".dll");
            if (File.Exists(file))
            {
                yield return file;
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            foreach (var candidate in EnumerateFiles(directory, $"lib{NativeMethods.LibraryName}.dylib", $"lib{NativeMethods.LibraryName}*.dylib"))
            {
                yield return candidate;
            }
        }
        else
        {
            foreach (var candidate in EnumerateFiles(directory, $"lib{NativeMethods.LibraryName}.so", $"lib{NativeMethods.LibraryName}.so*"))
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<string> EnumerateFiles(string directory, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            foreach (var file in EnumerateFilesForPattern(directory, pattern))
            {
                yield return file;
            }
        }
    }

    [ExcludeFromCodeCoverage] // The error-handling branches rely on OS-specific IO errors that cannot be portably triggered in tests.
    private static IEnumerable<string> EnumerateFilesForPattern(string directory, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            yield break;
        }

        if (!pattern.Contains('*', StringComparison.Ordinal) && !pattern.Contains('?', StringComparison.Ordinal))
        {
            var candidate = Path.Combine(directory, pattern);
            if (File.Exists(candidate))
            {
                yield return candidate;
            }

            yield break;
        }

        IEnumerable<string> matches;
        try
        {
            matches = Directory.EnumerateFiles(directory, pattern);
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        foreach (var match in matches)
        {
            yield return match;
        }
    }
}
