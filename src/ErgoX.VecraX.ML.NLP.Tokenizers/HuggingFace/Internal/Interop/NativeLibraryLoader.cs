using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

internal static class NativeLibraryLoader
{
    private const string WindowsX64 = "win-x64";
    private const string WindowsX86 = "win-x86";
    private const string WindowsArm64 = "win-arm64";
    private const string LinuxX64 = "linux-x64";
    private const string LinuxArm64 = "linux-arm64";
    private const string MacOsX64 = "osx-x64";
    private const string MacOsArm64 = "osx-arm64";
    private const string AndroidArm64 = "android-arm64";
    private const string AndroidX64 = "android-x64";
    private const string IosArm64 = "ios-arm64";

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, Resolve);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != "tokenx_bridge")
        {
            return IntPtr.Zero;
        }

        foreach (var rid in GetRuntimeIdentifiers())
        {
            foreach (var libraryFile in GetLibraryFileNames(rid))
            {
                foreach (var candidate in EnumerateCandidatePaths(rid, libraryFile, assembly))
                {
                    if (!File.Exists(candidate))
                    {
                        continue;
                    }

                    if (NativeLibrary.TryLoad(candidate, out var handle))
                    {
                        return handle;
                    }
                }
            }
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> GetRuntimeIdentifiers()
    {
        var emitted = false;

        foreach (var rid in GetPlatformRuntimeIdentifiers())
        {
            emitted = true;
            yield return rid;
        }

        if (!emitted && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            yield return WindowsX64;
        }
    }

    private static IEnumerable<string> GetPlatformRuntimeIdentifiers()
    {
        if (OperatingSystem.IsAndroid())
        {
            return GetAndroidRuntimeIdentifiers();
        }

        if (OperatingSystem.IsIOS())
        {
            return GetIosRuntimeIdentifiers();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsRuntimeIdentifiers();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxRuntimeIdentifiers();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacRuntimeIdentifiers();
        }

        return Array.Empty<string>();
    }

    private static IEnumerable<string> GetAndroidRuntimeIdentifiers()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            yield return AndroidArm64;
        }

        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            yield return AndroidX64;
        }
    }

    private static IEnumerable<string> GetIosRuntimeIdentifiers()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            yield return IosArm64;
        }
    }

    private static IEnumerable<string> GetWindowsRuntimeIdentifiers() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => new[] { WindowsX64 },
        Architecture.X86 => new[] { WindowsX86 },
        Architecture.Arm64 => new[] { WindowsArm64, WindowsX64 },
        _ => new[] { WindowsX64 }
    };

    private static IEnumerable<string> GetLinuxRuntimeIdentifiers() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => new[] { LinuxX64 },
        Architecture.Arm64 => new[] { LinuxArm64 },
        _ => new[] { LinuxX64 }
    };

    private static IEnumerable<string> GetMacRuntimeIdentifiers() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => new[] { MacOsX64 },
        Architecture.Arm64 => new[] { MacOsArm64 },
        _ => new[] { MacOsX64 }
    };

    private static IEnumerable<string> GetLibraryFileNames(string rid)
    {
        if (rid.StartsWith("win", StringComparison.Ordinal))
        {
            yield return "tokenx_bridge.dll";
            yield break;
        }

        if (rid.StartsWith("linux", StringComparison.Ordinal) || rid.StartsWith("android", StringComparison.Ordinal))
        {
            yield return "libtokenx_bridge.so";
            yield break;
        }

        if (rid.StartsWith("osx", StringComparison.Ordinal))
        {
            yield return "libtokenx_bridge.dylib";
            yield break;
        }

        if (rid.StartsWith("ios", StringComparison.Ordinal))
        {
            yield return "libtokenx_bridge.dylib";
            yield return "libtokenx_bridge.a";
            yield break;
        }

        yield return "tokenx_bridge.dll";
    }

    private static IEnumerable<string> EnumerateCandidatePaths(string rid, string fileName, Assembly assembly)
    {
        var assemblyDirectory = Path.GetDirectoryName(assembly.Location) ?? string.Empty;
        var runtimeRoot = Path.Combine("runtimes", rid, "native", fileName);

        yield return Path.Combine(AppContext.BaseDirectory, runtimeRoot);
        yield return Path.Combine(assemblyDirectory, runtimeRoot);
        yield return Path.Combine(AppContext.BaseDirectory, fileName);
        yield return Path.Combine(assemblyDirectory, fileName);
    }
}
