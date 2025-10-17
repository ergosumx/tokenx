using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

internal static class NativeLibraryLoader
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, Resolve);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != "tokenizers")
        {
            return IntPtr.Zero;
        }

        var rid = GetRuntimeIdentifier();
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", "tokenizers.dll"),
            Path.Combine(AppContext.BaseDirectory, "tokenizers.dll"),
            Path.Combine(Path.GetDirectoryName(assembly.Location) ?? string.Empty, "runtimes", rid, "native", "tokenizers.dll")
        };

        foreach (var path in paths)
        {
            if (File.Exists(path) && NativeLibrary.TryLoad(path, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    private static string GetRuntimeIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "win-x64",
                Architecture.X86 => "win-x86",
                Architecture.Arm64 => "win-arm64",
                _ => "win-x64"
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "linux-x64",
                Architecture.Arm64 => "linux-arm64",
                _ => "linux-x64"
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "osx-x64",
                Architecture.Arm64 => "osx-arm64",
                _ => "osx-x64"
            };
        }

        return "win-x64"; // Default fallback
    }
}
