using System;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;

internal sealed class NativeTokenizerHandle : SafeHandle
{
    private NativeTokenizerHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            NativeInteropProvider.Current.TokenizerFree(handle);
        }

        return true;
    }

    public static NativeTokenizerHandle Create(string json)
    {
        var ptr = NativeInteropProvider.Current.TokenizerCreateFromJson(json, out var status);
        if (ptr == IntPtr.Zero || status != 0)
        {
            var message = NativeInteropProvider.Current.GetLastErrorMessage() ?? "Failed to create tokenizer handle.";
            throw new InvalidOperationException(message);
        }

        var handle = new NativeTokenizerHandle();
        handle.SetHandle(ptr);
        return handle;
    }

    public static NativeTokenizerHandle CreateFromPretrained(string identifier, string? revision, string? authToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier must be provided.", nameof(identifier));
        }

        var ptr = NativeInteropProvider.Current.TokenizerCreateFromPretrained(identifier, revision, authToken, out var status);
        if (ptr == IntPtr.Zero || status != 0)
        {
            var message = NativeInteropProvider.Current.GetLastErrorMessage() ?? "Failed to load pretrained tokenizer.";
            throw new InvalidOperationException(message);
        }

        var handle = new NativeTokenizerHandle();
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
