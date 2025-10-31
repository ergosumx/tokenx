namespace ErgoX.TokenX.HuggingFace.Internal.Interop;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

internal static class NativeLibraryResolverRegistry
{
    private static readonly object SyncRoot = new();
    private static readonly Dictionary<Assembly, List<Func<string, Assembly, DllImportSearchPath?, IntPtr>>> ResolverMap = new();

    public static void Register(Assembly assembly, Func<string, Assembly, DllImportSearchPath?, IntPtr> resolver)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        lock (SyncRoot)
        {
            if (!ResolverMap.TryGetValue(assembly, out var resolvers))
            {
                resolvers = new List<Func<string, Assembly, DllImportSearchPath?, IntPtr>>();
                ResolverMap[assembly] = resolvers;
                NativeLibrary.SetDllImportResolver(assembly, (name, requestingAssembly, searchPath) => Dispatch(assembly, name, requestingAssembly, searchPath));
            }

            resolvers.Add(resolver);
        }
    }

    private static IntPtr Dispatch(Assembly assembly, string libraryName, Assembly requestingAssembly, DllImportSearchPath? searchPath)
    {
        Func<string, Assembly, DllImportSearchPath?, IntPtr>[] snapshot;
        lock (SyncRoot)
        {
            if (!ResolverMap.TryGetValue(assembly, out var resolvers) || resolvers.Count == 0)
            {
                return IntPtr.Zero;
            }

            snapshot = resolvers.ToArray();
        }

        foreach (var resolver in snapshot)
        {
            var handle = resolver(libraryName, requestingAssembly, searchPath);
            if (handle != IntPtr.Zero)
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }
}
