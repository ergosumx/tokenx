using System;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;

/// <summary>
/// Manages a native tokenizer pointer obtained from the native tokenizers library.
/// </summary>
/// <remarks>
/// This handle wraps a native tokenizer instance and ensures proper cleanup via the <see cref="SafeHandle"/> pattern.
/// Instances can be created from JSON configuration or loaded from pretrained HuggingFace models.
/// </remarks>
internal sealed class NativeTokenizerHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeTokenizerHandle"/> class.
    /// </summary>
    private NativeTokenizerHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    /// <summary>
    /// Gets a value indicating whether this handle points to an invalid (null) native object.
    /// </summary>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Releases the native tokenizer resource.
    /// </summary>
    /// <returns>True if the handle was successfully released; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            NativeInteropProvider.Current.TokenizerFree(handle);
        }

        return true;
    }

    /// <summary>
    /// Creates a tokenizer handle from JSON configuration.
    /// </summary>
    /// <param name="json">The JSON tokenizer configuration.</param>
    /// <returns>A new handle wrapping the native tokenizer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when tokenizer creation fails.</exception>
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

    /// <summary>
    /// Creates a tokenizer handle from a pretrained HuggingFace model identifier.
    /// </summary>
    /// <param name="identifier">The model identifier (e.g., "gpt2", "roberta-base").</param>
    /// <param name="revision">Optional revision identifier (e.g., "main", specific commit hash).</param>
    /// <param name="authToken">Optional authentication token for private models.</param>
    /// <returns>A new handle wrapping the downloaded and loaded tokenizer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when loading the pretrained tokenizer fails.</exception>
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

    /// <summary>
    /// Invokes a callback with the native pointer, ensuring proper reference counting.
    /// </summary>
    /// <param name="invoker">The callback to invoke with the native pointer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invoker"/> is null.</exception>
    /// <remarks>
    /// This method safely exposes the underlying native pointer for interop calls
    /// while maintaining reference counting guarantees via DangerousAddRef and DangerousRelease.
    /// </remarks>
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
