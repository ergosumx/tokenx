using System;
using System.Collections.Generic;
using System.Linq;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Decoders;

/// <summary>
/// Sequence decoder that composes multiple decoders and applies them in order.
/// </summary>
public sealed class SequenceDecoder : IDecoder
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new Sequence decoder from the provided decoders.
    /// </summary>
    /// <param name="decoders">The decoders to compose. They are applied in the provided order.</param>
    /// <exception cref="ArgumentNullException">Thrown when decoders is null.</exception>
    /// <exception cref="ArgumentException">Thrown when decoders is empty or contains null entries.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native decoder cannot be created.</exception>
    public SequenceDecoder(IEnumerable<IDecoder> decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);

        var decoderArray = decoders.ToArray();
        if (decoderArray.Length == 0)
        {
            throw new ArgumentException("At least one decoder must be provided.", nameof(decoders));
        }

        var handles = new IntPtr[decoderArray.Length];
        for (var i = 0; i < decoderArray.Length; i++)
        {
            var decoder = decoderArray[i];
            if (decoder is null)
            {
                throw new ArgumentException("Decoder entries cannot be null.", nameof(decoders));
            }

            handles[i] = decoder.Handle;
        }

        _handle = NativeMethods.SequenceDecoderNew(handles, (nuint)handles.Length, out var status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            var error = NativeMethods.GetLastErrorMessage();
            throw new InvalidOperationException($"Failed to create Sequence decoder. Status: {status}. {error}");
        }
    }

    /// <summary>
    /// Creates a new Sequence decoder from the provided decoders.
    /// </summary>
    /// <param name="decoders">The decoders to compose. They are applied in the provided order.</param>
    public SequenceDecoder(params IDecoder[] decoders)
        : this((IEnumerable<IDecoder>)decoders)
    {
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
    /// Releases the resources used by the decoder.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.SequenceDecoderFree(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~SequenceDecoder()
    {
        Dispose(false);
    }
}
