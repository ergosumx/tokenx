namespace ErgoX.TokenX.HuggingFace;

using System;
using System.Runtime.InteropServices;
using ErgoX.TokenX.HuggingFace.Abstractions;
using ErgoX.TokenX.HuggingFace.Internal;
using ErgoX.TokenX.HuggingFace.Internal.Interop;

/// <summary>
/// Provides a managed wrapper around a native tokenizer model implementation.
/// </summary>
public class TokenizerModel : IModel
{
    private readonly NativeModelHandle _handle;
    private readonly INativeInterop _interop;
    private bool _disposed;

    internal TokenizerModel(NativeModelHandle handle, INativeInterop interop)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _interop = interop ?? throw new ArgumentNullException(nameof(interop));
    }

    /// <summary>
    /// Creates a tokenizer model from a native JSON payload.
    /// </summary>
    /// <param name="json">The model JSON.</param>
    /// <returns>A <see cref="TokenizerModel"/> instance wrapping the native model.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static TokenizerModel FromJson(string json)
    {
        var interop = NativeInteropProvider.Current;
        var handle = NativeModelHandle.Create(json, interop);
        return new TokenizerModel(handle, interop);
    }

    /// <summary>
    /// Gets the native model type identifier (e.g. <c>BPE</c>, <c>WordPiece</c>).
    /// </summary>
    public string Type => InvokeWithHandle(ptr =>
    {
        var nativePtr = _interop.TokenizersModelGetType(ptr, out var status);
        if (nativePtr == IntPtr.Zero || status != 0)
        {
            throw CreateNativeException("Model type retrieval failed.");
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
    /// Serializes the model back to JSON.
    /// </summary>
    /// <param name="pretty">True to request pretty-printed JSON.</param>
    /// <returns>The serialized JSON payload.</returns>
    public string ToJson(bool pretty = false)
        => InvokeWithHandle(ptr =>
        {
            var nativePtr = _interop.TokenizersModelToJson(ptr, pretty, out var status);
            if (nativePtr == IntPtr.Zero || status != 0)
            {
                throw CreateNativeException("Model serialization failed.");
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

    IntPtr IModel.Handle
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources associated with the model.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is invoked from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _handle?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Finalizes the model in case <see cref="Dispose()"/> was not called.
    /// </summary>
    ~TokenizerModel()
    {
        Dispose(disposing: false);
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
            throw new ObjectDisposedException(nameof(TokenizerModel));
        }
    }
}

