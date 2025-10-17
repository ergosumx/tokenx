using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using System;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// Prepend scheme for Metaspace decoder.
/// </summary>
public enum PrependScheme : byte
{
    /// <summary>
    /// Always prepend the replacement character.
    /// </summary>
    Always = 0,

    /// <summary>
    /// Prepend only to the first token.
    /// </summary>
    First = 1,

    /// <summary>
    /// Never prepend the replacement character.
    /// </summary>
    Never = 2
}

/// <summary>
/// Metaspace decoder for SentencePiece-style whitespace handling.
/// </summary>
/// <remarks>
/// This decoder replaces the meta symbol (typically '▁') with actual spaces.
/// It's used in conjunction with the Metaspace pre-tokenizer.
/// </remarks>
public sealed class MetaspaceDecoder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Gets the replacement character used to represent spaces.
    /// </summary>
    public char Replacement { get; }

    /// <summary>
    /// Gets the prepend scheme.
    /// </summary>
    public PrependScheme PrependScheme { get; }

    /// <summary>
    /// Gets whether to split on the replacement character.
    /// </summary>
    public bool Split { get; }

    /// <summary>
    /// Creates a new Metaspace decoder with default settings.
    /// </summary>
    /// <remarks>
    /// Uses default replacement: '▁' (U+2581), prepend scheme: Always, and split enabled.
    /// </remarks>
    public MetaspaceDecoder()
        : this('▁', PrependScheme.Always, true)
    {
    }

    /// <summary>
    /// Creates a new Metaspace decoder with custom settings.
    /// </summary>
    /// <param name="replacement">The replacement character. Must be exactly one character.</param>
    /// <param name="prependScheme">Whether to add a space to the first word if there isn't already one.</param>
    /// <param name="split">Whether to split on the replacement character.</param>
    /// <exception cref="InvalidOperationException">Thrown when the decoder cannot be created.</exception>
    public MetaspaceDecoder(char replacement, PrependScheme prependScheme, bool split)
    {
        Replacement = replacement;
        PrependScheme = prependScheme;
        Split = split;

        _handle = NativeMethods.MetaspaceDecoderNew(
            replacement.ToString(),
            (byte)prependScheme,
            split,
            out int status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create Metaspace decoder. Status: {status}. {error}");
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
                NativeMethods.MetaspaceDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~MetaspaceDecoder()
    {
        Dispose(false);
    }
}
