namespace ErgoX.TokenX.SentencePiece.Processing;

using System;
using System.Runtime.InteropServices;
using ErgoX.TokenX.SentencePiece.Exceptions;
using ErgoX.TokenX.SentencePiece.Internal.Interop;
using ErgoX.TokenX.SentencePiece.Options;

/// <summary>
/// The main SentencePiece tokenizer processor.
/// Provides methods for encoding text into token IDs or pieces, decoding tokens back to text,
/// and various tokenization operations including N-best, sampling, and scoring.
/// Implements <see cref="IDisposable"/> to properly manage native resources.
/// </summary>
public sealed partial class SentencePieceProcessor : IDisposable
{
    private static readonly int SpcBytesStructSize = Marshal.SizeOf<NativeMethods.SpcBytes>();

    private readonly NativeMethods.ProcessorSafeHandle handle;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SentencePieceProcessor"/> class.
    /// Creates an empty processor that must be configured by calling a load method.
    /// </summary>
    /// <exception cref="SentencePieceException">Thrown if native processor allocation fails.</exception>
    public SentencePieceProcessor()
    {
        var pointer = NativeMethods.spc_sentencepiece_processor_create();
        if (pointer == IntPtr.Zero)
        {
            throw new SentencePieceException("Failed to allocate SentencePiece processor.", SentencePieceStatusCode.Internal);
        }

        handle = new NativeMethods.ProcessorSafeHandle();
        handle.Initialize(pointer);
    }

    /// <summary>
    /// Disposes the processor and frees native resources.
    /// Can be called multiple times safely.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        handle.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    public void Load(string modelPath)
    {
        ThrowIfDisposed();
        using var path = new InteropUtilities.NativeUtf8(modelPath);
        var status = NativeMethods.spc_sentencepiece_processor_load_from_file(handle, path.View);
        InteropUtilities.EnsureSuccess(status);
    }

    public void Load(ReadOnlySpan<byte> serializedModel)
    {
        ThrowIfDisposed();
        using var buffer = new InteropUtilities.NativeBuffer(serializedModel);
        var status = NativeMethods.spc_sentencepiece_processor_load_from_serialized_proto(handle, buffer.View.Data, buffer.View.Length);
        InteropUtilities.EnsureSuccess(status);
    }

    public void SetEncodeExtraOptions(string extraOptions)
    {
        ThrowIfDisposed();
        using var value = new InteropUtilities.NativeUtf8(extraOptions);
        var status = NativeMethods.spc_sentencepiece_processor_set_encode_extra_options(handle, value.View);
        InteropUtilities.EnsureSuccess(status);
    }

    public void SetDecodeExtraOptions(string extraOptions)
    {
        ThrowIfDisposed();
        using var value = new InteropUtilities.NativeUtf8(extraOptions);
        var status = NativeMethods.spc_sentencepiece_processor_set_decode_extra_options(handle, value.View);
        InteropUtilities.EnsureSuccess(status);
    }

    public void SetVocabulary(IEnumerable<string> pieces)
    {
        ThrowIfDisposed();
        using var nativePieces = new InteropUtilities.NativeUtf8Array(pieces ?? Array.Empty<string>());
        var status = NativeMethods.spc_sentencepiece_processor_set_vocabulary(handle, nativePieces.Pointer, nativePieces.Length);
        InteropUtilities.EnsureSuccess(status);
    }

    public void ResetVocabulary()
    {
        ThrowIfDisposed();
        var status = NativeMethods.spc_sentencepiece_processor_reset_vocabulary(handle);
        InteropUtilities.EnsureSuccess(status);
    }

    public void LoadVocabulary(string filePath, int threshold = 0)
    {
        ThrowIfDisposed();
        using var path = new InteropUtilities.NativeUtf8(filePath);
        var status = NativeMethods.spc_sentencepiece_processor_load_vocabulary(handle, path.View, threshold);
        InteropUtilities.EnsureSuccess(status);
    }

    public int PieceCount
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.spc_sentencepiece_processor_get_piece_size(handle);
        }
    }

    public int PieceSize => PieceCount;

    public int VocabSize => PieceCount;

    public int PieceToId(string piece)
    {
        ThrowIfDisposed();
        using var value = new InteropUtilities.NativeUtf8(piece);
        return NativeMethods.spc_sentencepiece_processor_piece_to_id(handle, value.View);
    }

    public string IdToPiece(int id)
    {
        ThrowIfDisposed();
        var status = NativeMethods.spc_sentencepiece_processor_id_to_piece(handle, id, out var bytes);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToStringAndDestroy(ref bytes);
    }

    public float GetScore(int id)
    {
        ThrowIfDisposed();
        return NativeMethods.spc_sentencepiece_processor_get_score(handle, id);
    }

    public bool IsUnknown(int id)
    {
        ThrowIfDisposed();
        return NativeMethods.spc_sentencepiece_processor_is_unknown(handle, id);
    }

    public bool IsControl(int id)
    {
        ThrowIfDisposed();
        return NativeMethods.spc_sentencepiece_processor_is_control(handle, id);
    }

    public bool IsUnused(int id)
    {
        ThrowIfDisposed();
        return NativeMethods.spc_sentencepiece_processor_is_unused(handle, id);
    }

    public bool IsByte(int id)
    {
        ThrowIfDisposed();
        return NativeMethods.spc_sentencepiece_processor_is_byte(handle, id);
    }

    public int UnknownId
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.spc_sentencepiece_processor_unk_id(handle);
        }
    }

    public int BosId
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.spc_sentencepiece_processor_bos_id(handle);
        }
    }

    public int EosId
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.spc_sentencepiece_processor_eos_id(handle);
        }
    }

    public int PadId
    {
        get
        {
            ThrowIfDisposed();
            return NativeMethods.spc_sentencepiece_processor_pad_id(handle);
        }
    }

    public byte[] SerializeModel()
    {
        ThrowIfDisposed();
        var status = NativeMethods.spc_sentencepiece_processor_serialized_model_proto(handle, out var bytes);
        InteropUtilities.EnsureSuccess(status);
        return InteropUtilities.BytesToArrayAndDestroy(ref bytes);
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(SentencePieceProcessor));
        }
    }

    private static unsafe T InvokeWithEncodeOptions<T>(EncodeOptions? options, Func<IntPtr, T> callback)
    {
        if (options is null)
        {
            return callback(IntPtr.Zero);
        }

        var native = InteropUtilities.CreateEncodeOptions(options);
        var pointer = (IntPtr)(&native);
        return callback(pointer);
    }

    private static unsafe T InvokeWithSampleOptions<T>(SampleEncodeAndScoreOptions? options, Func<IntPtr, T> callback)
    {
        if (options is null)
        {
            return callback(IntPtr.Zero);
        }

        var native = InteropUtilities.CreateSampleOptions(options);
        var pointer = (IntPtr)(&native);
        return callback(pointer);
    }

    private static IReadOnlyList<byte[]> BytesArrayToByteArraysAndDestroy(ref NativeMethods.SpcBytesArray array)
    {
        try
        {
            if (array.Length == 0 || array.Items == IntPtr.Zero)
            {
                return Array.Empty<byte[]>();
            }

            var result = new byte[(int)array.Length][];
            var current = array.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcBytes>(current);
                result[i] = CopyBytes(entry);
                current = IntPtr.Add(current, SpcBytesStructSize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_bytes_array_destroy(ref array);
        }
    }

    private static byte[] CopyBytes(NativeMethods.SpcBytes bytes)
    {
        if (bytes.Data == IntPtr.Zero || bytes.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var buffer = new byte[(int)bytes.Length];
        Marshal.Copy(bytes.Data, buffer, 0, buffer.Length);
        return buffer;
    }
}

