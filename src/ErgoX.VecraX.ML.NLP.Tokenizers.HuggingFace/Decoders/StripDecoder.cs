using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// Strip decoder that removes n characters from the left or right of each token.
/// </summary>
/// <remarks>
/// This decoder strips a specified number of characters from either side of each token.
/// Useful for removing padding or special characters added during tokenization.
/// </remarks>
public sealed class StripDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the character to strip.
    /// </summary>
    public char Content { get; }

    /// <summary>
    /// Gets the number of characters to strip from the left.
    /// </summary>
    public nuint Left { get; }

    /// <summary>
    /// Gets the number of characters to strip from the right.
    /// </summary>
    public nuint Right { get; }

    /// <summary>
    /// Creates a new Strip decoder with default settings.
    /// </summary>
    /// <remarks>
    /// Uses default content: ' ' (space), left: 0, right: 0.
    /// </remarks>
    public StripDecoder()
        : this(' ', 0, 0)
    {
    }

    /// <summary>
    /// Creates a new Strip decoder with custom settings.
    /// </summary>
    /// <param name="content">The character to strip.</param>
    /// <param name="left">Number of characters to strip from the left.</param>
    /// <param name="right">Number of characters to strip from the right.</param>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public StripDecoder(char content, nuint left, nuint right)
    {
        Content = content;
        Left = left;
        Right = right;

        _handle = NativeMethods.StripDecoderNew(content.ToString(), left, right, out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create Strip decoder. Status: {status}. {error}");
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
                NativeMethods.StripDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~StripDecoder()
    {
        Dispose(false);
    }
}
