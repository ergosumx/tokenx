namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;

using System;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

internal sealed class NativeDecoderHandle : SafeHandle
{
    private readonly INativeInterop _interop;

    private NativeDecoderHandle(INativeInterop interop)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        _interop = interop;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            _interop.TokenizersDecoderFree(handle);
        }

        return true;
    }

    public static NativeDecoderHandle Create(string json, INativeInterop interop)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Decoder JSON must be provided.", nameof(json));
        }

        ArgumentNullException.ThrowIfNull(interop);

        var ptr = interop.TokenizersDecoderFromJson(json, out var status);
        if (ptr == IntPtr.Zero || status != 0)
        {
            var message = interop.GetLastErrorMessage() ?? "Failed to create tokenizer decoder.";
            throw new InvalidOperationException(message);
        }

        var handle = new NativeDecoderHandle(interop);
        handle.SetHandle(ptr);
        return handle;
    }

    public T InvokeWithHandle<T>(Func<IntPtr, T> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        var addedRef = false;
        try
        {
            DangerousAddRef(ref addedRef);
            return invoker(handle);
        }
        finally
        {
            if (addedRef)
            {
                DangerousRelease();
            }
        }
    }

    public void InvokeWithHandle(Action<IntPtr> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        InvokeWithHandle(ptr =>
        {
            invoker(ptr);
            return true;
        });
    }
}
