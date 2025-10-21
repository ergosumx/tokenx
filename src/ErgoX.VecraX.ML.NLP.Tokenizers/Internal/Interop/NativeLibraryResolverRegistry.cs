namespace ErgoX.VecraX.ML.NLP.Tokenizers.Internal.Interop;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

internal static class NativeLibraryResolverRegistry
{
    private static readonly object SyncRoot = new();
    private static readonly List<Func<string, Assembly, DllImportSearchPath?, IntPtr>> Resolvers = new();
    private static bool isRegistered;

    internal static void Register(Func<string, Assembly, DllImportSearchPath?, IntPtr> resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        EnsureRegistered();

        lock (SyncRoot)
        {
            Resolvers.Add(resolver);
        }
    }

    private static void EnsureRegistered()
    {
        if (Volatile.Read(ref isRegistered))
        {
            return;
        }

        lock (SyncRoot)
        {
            if (isRegistered)
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolverRegistry).Assembly, Dispatch);
            isRegistered = true;
        }
    }

    private static IntPtr Dispatch(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        Func<string, Assembly, DllImportSearchPath?, IntPtr>[] snapshot;
        lock (SyncRoot)
        {
            snapshot = Resolvers.ToArray();
        }

        foreach (var resolver in snapshot)
        {
            var handle = resolver(libraryName, assembly, searchPath);
            if (handle != IntPtr.Zero)
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }
}
