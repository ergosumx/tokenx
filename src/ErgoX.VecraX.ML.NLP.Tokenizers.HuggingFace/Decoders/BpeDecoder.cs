using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// BPE decoder that replaces end-of-word suffixes with whitespace.
/// </summary>
/// <remarks>
/// This decoder is used to decode byte-pair encoding tokens by replacing
/// the specified suffix (typically "&lt;/w&gt;") with whitespace during decoding.
/// </remarks>
public sealed class BpeDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the suffix string that characterizes end-of-word.
    /// </summary>
    public string Suffix { get; }

    /// <summary>
    /// Creates a new BPE decoder with default suffix.
    /// </summary>
    /// <remarks>
    /// Uses default suffix: "&lt;/w&gt;".
    /// </remarks>
    public BpeDecoder()
        : this("</w>")
    {
    }

    /// <summary>
    /// Creates a new BPE decoder with custom suffix.
    /// </summary>
    /// <param name="suffix">The suffix that characterizes an end-of-word. Will be replaced by whitespace during decoding.</param>
    /// <exception cref="ArgumentNullException">Thrown when suffix is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public BpeDecoder(string suffix)
    {
        ArgumentNullException.ThrowIfNull(suffix);

        Suffix = suffix;

        _handle = NativeMethods.BpeDecoderNew(suffix, out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create BPE decoder. Status: {status}. {error}");
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
                NativeMethods.BpeDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~BpeDecoder()
    {
        Dispose(false);
    }
}
