namespace ErgoX.TokenX.HuggingFace.Internal;

using System;
using System.Runtime.InteropServices;
using ErgoX.TokenX.HuggingFace.Internal.Interop;

/// <summary>
/// Manages a native decoder pointer obtained from the native tokenizers library.
/// </summary>
/// <remarks>
/// This handle wraps a native decoder instance used for converting token IDs back to text.
/// Each decoder is associated with a specific <see cref="INativeInterop"/> provider for proper lifecycle management.
/// </remarks>
internal sealed class NativeDecoderHandle : SafeHandle
{
    private readonly INativeInterop _interop;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeDecoderHandle"/> class.
    /// </summary>
    /// <param name="interop">The native interop provider responsible for managing this handle.</param>
    private NativeDecoderHandle(INativeInterop interop)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        _interop = interop;
    }

    /// <summary>
    /// Gets a value indicating whether this handle points to an invalid (null) native object.
    /// </summary>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Releases the native decoder resource.
    /// </summary>
    /// <returns>True if the handle was successfully released; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            _interop.TokenizersDecoderFree(handle);
        }

        return true;
    }

    /// <summary>
    /// Creates a decoder handle from JSON configuration.
    /// </summary>
    /// <param name="json">The JSON decoder configuration.</param>
    /// <param name="interop">The native interop provider for this handle.</param>
    /// <returns>A new handle wrapping the native decoder.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interop"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when decoder creation fails.</exception>
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

    /// <summary>
    /// Invokes a callback with the native pointer, ensuring proper reference counting.
    /// </summary>
    /// <typeparam name="T">The return type of the callback.</typeparam>
    /// <param name="invoker">The callback to invoke with the native pointer.</param>
    /// <returns>The result returned by the callback.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invoker"/> is null.</exception>
    /// <remarks>
    /// This method safely exposes the underlying native pointer for interop calls
    /// while maintaining reference counting guarantees via DangerousAddRef and DangerousRelease.
    /// </remarks>
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

