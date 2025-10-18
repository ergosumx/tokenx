using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// Fuse decoder that simply concatenates all tokens into a single string.
/// </summary>
/// <remarks>
/// This decoder fuses every token into a single string. This is typically the last step
/// of decoding, and this decoder exists only if there is need to add other decoders after fusion.
/// </remarks>
public sealed class FuseDecoder : IDecoder
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new Fuse decoder.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public FuseDecoder()
    {
        _handle = NativeMethods.FuseDecoderNew(out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create Fuse decoder. Status: {status}. {error}");
        }
    }

    /// <summary>
    /// Gets the native handle for this decoder.
    /// </summary>
    public IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    /// <summary>
    /// Releases all resources used by the decoder.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.FuseDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~FuseDecoder()
    {
        Dispose(false);
    }
}
