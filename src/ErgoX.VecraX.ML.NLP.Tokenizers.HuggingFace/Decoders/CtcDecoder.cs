using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// CTC (Connectionist Temporal Classification) decoder.
/// </summary>
/// <remarks>
/// This decoder is used for CTC-based models. It removes padding tokens and
/// replaces word delimiter tokens with spaces, optionally cleaning up tokenization artifacts.
/// </remarks>
public sealed class CtcDecoder : IDecoder
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the pad token used by CTC to delimit a new token.
    /// </summary>
    public string PadToken { get; }

    /// <summary>
    /// Gets the word delimiter token that will be replaced by a space.
    /// </summary>
    public string WordDelimiterToken { get; }

    /// <summary>
    /// Gets whether to cleanup tokenization artifacts.
    /// </summary>
    public bool Cleanup { get; }

    /// <summary>
    /// Creates a new CTC decoder with default settings.
    /// </summary>
    /// <remarks>
    /// Uses default pad token: "&lt;pad&gt;", word delimiter: "|", and cleanup enabled.
    /// </remarks>
    public CtcDecoder()
        : this("<pad>", "|", true)
    {
    }

    /// <summary>
    /// Creates a new CTC decoder with custom settings.
    /// </summary>
    /// <param name="padToken">The pad token used by CTC to delimit a new token.</param>
    /// <param name="wordDelimiterToken">The word delimiter token. It will be replaced by a space.</param>
    /// <param name="cleanup">Whether to cleanup tokenization artifacts (spaces before punctuation, etc.).</param>
    /// <exception cref="ArgumentNullException">Thrown when padToken or wordDelimiterToken is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public CtcDecoder(string padToken, string wordDelimiterToken, bool cleanup)
    {
        ArgumentNullException.ThrowIfNull(padToken);
        ArgumentNullException.ThrowIfNull(wordDelimiterToken);

        PadToken = padToken;
        WordDelimiterToken = wordDelimiterToken;
        Cleanup = cleanup;

        _handle = NativeMethods.CtcDecoderNew(padToken, wordDelimiterToken, cleanup, out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create CTC decoder. Status: {status}. {error}");
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
                NativeMethods.CtcDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~CtcDecoder()
    {
        Dispose(false);
    }
}
