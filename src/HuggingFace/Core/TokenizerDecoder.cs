namespace ErgoX.TokenX.HuggingFace;

using System;
using System.Runtime.InteropServices;
using ErgoX.TokenX.HuggingFace.Abstractions;
using ErgoX.TokenX.HuggingFace.Internal;
using ErgoX.TokenX.HuggingFace.Internal.Interop;

/// <summary>
/// Provides a managed wrapper around a native tokenizer decoder implementation.
/// </summary>
public sealed class TokenizerDecoder : IDecoder
{
    private readonly NativeDecoderHandle _handle;
    private readonly INativeInterop _interop;
    private bool _disposed;

    private TokenizerDecoder(NativeDecoderHandle handle, INativeInterop interop)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _interop = interop ?? throw new ArgumentNullException(nameof(interop));
    }

    /// <summary>
    /// Creates a tokenizer decoder from a native JSON payload.
    /// </summary>
    /// <param name="json">The decoder JSON.</param>
    /// <returns>A <see cref="TokenizerDecoder"/> instance wrapping the native decoder.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static TokenizerDecoder FromJson(string json)
    {
        var interop = NativeInteropProvider.Current;
        var handle = NativeDecoderHandle.Create(json, interop);
        return new TokenizerDecoder(handle, interop);
    }

    /// <summary>
    /// Gets the native decoder type identifier (e.g. <c>ByteLevel</c>, <c>WordPiece</c>).
    /// </summary>
    public string Type => InvokeWithHandle(ptr =>
    {
        var nativePtr = _interop.TokenizersDecoderGetType(ptr, out var status);
        if (nativePtr == IntPtr.Zero || status != 0)
        {
            throw CreateNativeException("Decoder type retrieval failed.");
        }

        try
        {
            return Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
        }
        finally
        {
            _interop.FreeString(nativePtr);
        }
    });

    /// <summary>
    /// Serializes the decoder back to JSON.
    /// </summary>
    /// <param name="pretty">True to request pretty-printed JSON.</param>
    /// <returns>The serialized JSON payload.</returns>
    public string ToJson(bool pretty = false)
        => InvokeWithHandle(ptr =>
        {
            var nativePtr = _interop.TokenizersDecoderToJson(ptr, pretty, out var status);
            if (nativePtr == IntPtr.Zero || status != 0)
            {
                throw CreateNativeException("Decoder serialization failed.");
            }

            try
            {
                return Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
            }
            finally
            {
                _interop.FreeString(nativePtr);
            }
        });

    IntPtr IDecoder.Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle.InvokeWithHandle(static ptr => ptr);
        }
    }

    internal T InvokeWithHandle<T>(Func<IntPtr, T> invoker)
    {
        ThrowIfDisposed();
        return _handle.InvokeWithHandle(invoker);
    }

    internal void InvokeWithHandle(Action<IntPtr> invoker)
    {
        ThrowIfDisposed();
        _handle.InvokeWithHandle(invoker);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _handle.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private InvalidOperationException CreateNativeException(string message)
    {
        var details = _interop.GetLastErrorMessage();
        return details is null ? new InvalidOperationException(message) : new InvalidOperationException($"{message}: {details}");
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TokenizerDecoder));
        }
    }
}

