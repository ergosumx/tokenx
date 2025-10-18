using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// Replace decoder that performs pattern-based text replacement.
/// </summary>
/// <remarks>
/// This decoder is to be used in tandem with the Replace pre-tokenizer.
/// It replaces occurrences of a pattern with specified content.
/// </remarks>
public sealed class ReplaceDecoder : IDecoder
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the pattern to search for (regex or literal string).
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Gets the content to replace the pattern with.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Creates a new Replace decoder.
    /// </summary>
    /// <param name="pattern">The pattern to search for (regex or literal string).</param>
    /// <param name="content">The content to replace the pattern with.</param>
    /// <exception cref="ArgumentNullException">Thrown when pattern or content is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public ReplaceDecoder(string pattern, string content)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(content);

        Pattern = pattern;
        Content = content;

        _handle = NativeMethods.ReplaceDecoderNew(pattern, content, out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create Replace decoder. Status: {status}. {error}");
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
                NativeMethods.ReplaceDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~ReplaceDecoder()
    {
        Dispose(false);
    }
}
