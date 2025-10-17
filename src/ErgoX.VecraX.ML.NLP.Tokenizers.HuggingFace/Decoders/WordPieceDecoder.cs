using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// WordPiece decoder that removes subword prefixes and cleans up tokenization artifacts.
/// </summary>
/// <remarks>
/// This decoder removes the subword prefix (typically "##") from tokens and optionally
/// cleans up tokenization artifacts like spaces before punctuation.
/// </remarks>
public sealed class WordPieceDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the prefix used for subwords that are not a beginning-of-word.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets whether to cleanup tokenization artifacts.
    /// </summary>
    public bool Cleanup { get; }

    /// <summary>
    /// Creates a new WordPiece decoder with default settings.
    /// </summary>
    /// <remarks>
    /// Uses default prefix: "##" and cleanup enabled.
    /// </remarks>
    public WordPieceDecoder()
        : this("##", true)
    {
    }

    /// <summary>
    /// Creates a new WordPiece decoder with custom settings.
    /// </summary>
    /// <param name="prefix">The prefix to use for subwords that are not a beginning-of-word.</param>
    /// <param name="cleanup">Whether to cleanup tokenization artifacts (spaces before punctuation, etc.).</param>
    /// <exception cref="ArgumentNullException">Thrown when prefix is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public WordPieceDecoder(string prefix, bool cleanup)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        Prefix = prefix;
        Cleanup = cleanup;

        _handle = NativeMethods.WordPieceDecoderNew(prefix, cleanup, out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create WordPiece decoder. Status: {status}. {error}");
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
                NativeMethods.WordPieceDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~WordPieceDecoder()
    {
        Dispose(false);
    }
}
