namespace ErgoX.TokenX.HuggingFace.Internal.Interop;

using System;
using System.Threading;

internal static class NativeInteropProvider
{
    private static INativeInterop current = NativeMethodsBridge.Instance;

    public static INativeInterop Current => Volatile.Read(ref current);

    public static IDisposable Override(INativeInterop replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        var previous = Interlocked.Exchange(ref current, replacement);
        return new Reverter(previous);
    }

    private sealed class Reverter : IDisposable
    {
        private INativeInterop? previous;

        public Reverter(INativeInterop previous)
        {
            this.previous = previous;
        }

        public void Dispose()
        {
            var prior = Interlocked.Exchange(ref current, previous ?? NativeMethodsBridge.Instance);
            previous = null;
            _ = prior; // ensure JIT keeps side-effect minimal
        }
    }
}

