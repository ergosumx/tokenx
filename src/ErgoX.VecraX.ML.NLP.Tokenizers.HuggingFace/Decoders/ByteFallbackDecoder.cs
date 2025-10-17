using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// ByteFallback decoder for converting byte tokens to strings.
/// </summary>
/// <remarks>
/// ByteFallback is a simple trick which converts tokens looking like "&lt;0x61&gt;" to pure bytes,
/// and attempts to make them into a string. If the tokens cannot be decoded, you will get � (replacement character)
/// instead for each inconvertible byte token.
/// </remarks>
public sealed class ByteFallbackDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new ByteFallback decoder.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public ByteFallbackDecoder()
    {
        _handle = NativeMethods.ByteFallbackDecoderNew(out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create ByteFallback decoder. Status: {status}. {error}");
        }
    }

    /// <summary>
    /// Gets the native handle for this decoder.
    /// </summary>
    internal IntPtr Handle
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
                NativeMethods.ByteFallbackDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~ByteFallbackDecoder()
    {
        Dispose(false);
    }
}
