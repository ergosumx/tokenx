namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken.Internal.Interop;

using System;
using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    private const string LibraryName = "tt_bridge";

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeBytes
    {
        internal readonly IntPtr Data;
        internal readonly nuint Length;

        internal NativeBytes(IntPtr data, nuint length)
        {
            Data = data;
            Length = length;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeMergeEntry
    {
        internal readonly NativeBytes Bytes;
        internal readonly uint Rank;

        internal NativeMergeEntry(NativeBytes bytes, uint rank)
        {
            Bytes = bytes;
            Rank = rank;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeSpecialToken
    {
        internal readonly IntPtr Text;
        internal readonly uint Rank;

        internal NativeSpecialToken(IntPtr text, uint rank)
        {
            Text = text;
            Rank = rank;
        }
    }

    [LibraryImport(LibraryName, EntryPoint = "tiktoken_get_last_error")]
    internal static partial IntPtr TiktokenGetLastError();

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_new", StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial IntPtr CoreBpeNew(
        string pattern,
        NativeMergeEntry* merges,
        nuint mergesLength,
        NativeSpecialToken* specials,
        nuint specialsLength,
        uint explicitVocabularySize);

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_free")]
    internal static partial void CoreBpeFree(IntPtr handle);

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_encode_ordinary", StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial IntPtr CoreBpeEncodeOrdinary(IntPtr handle, string text);

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_encode", StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial IntPtr CoreBpeEncode(
        IntPtr handle,
        string text,
        IntPtr* allowedSpecial,
        nuint allowedSpecialLength);

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_decode")]
    internal static unsafe partial IntPtr CoreBpeDecode(IntPtr handle, uint* tokens, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "ttk_core_bpe_decode_bytes")]
    internal static unsafe partial IntPtr CoreBpeDecodeBytes(IntPtr handle, uint* tokens, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "ttk_encoding_get_len")]
    internal static partial nuint EncodingGetLength(IntPtr encoding);

    [LibraryImport(LibraryName, EntryPoint = "ttk_encoding_try_copy_tokens")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool EncodingTryCopyTokens(IntPtr encoding, uint* destination, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "ttk_encoding_free")]
    internal static partial void EncodingFree(IntPtr encoding);

    [LibraryImport(LibraryName, EntryPoint = "ttk_string_get_len")]
    internal static partial nuint StringGetLength(IntPtr value);

    [LibraryImport(LibraryName, EntryPoint = "ttk_string_try_copy_bytes")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool StringTryCopyBytes(IntPtr value, byte* destination, nuint length);

    [LibraryImport(LibraryName, EntryPoint = "ttk_string_free")]
    internal static partial void StringFree(IntPtr value);

    internal static string? ConsumeLastError()
    {
        var ptr = TiktokenGetLastError();
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }
}
