using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// ByteLevel decoder for converting byte-level tokens back to text.
/// </summary>
/// <remarks>
/// This decoder is to be used in tandem with the <see cref="PreTokenizers.ByteLevelPreTokenizer"/>.
/// It reverses the byte-level encoding by mapping bytes back to their original Unicode characters.
/// </remarks>
public sealed class ByteLevelDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new ByteLevel decoder.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public ByteLevelDecoder()
    {
        _handle = NativeMethods.ByteLevelDecoderNew(out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create ByteLevel decoder. Status: {status}. {error}");
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
                NativeMethods.ByteLevelDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~ByteLevelDecoder()
    {
        Dispose(false);
    }
}
