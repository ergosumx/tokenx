namespace ErgoX.TokenX.SentencePiece.Internal.Interop;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Contains P/Invoke declarations for the native SentencePiece C library.
/// This internal class mirrors the API surface exposed by the reduced native bindings.
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class NativeMethods
{
    internal const string LibraryName = "sentencepiece_c";

    internal enum SpcStatusCode
    {
        Ok = 0,
        Cancelled = 1,
        Unknown = 2,
        InvalidArgument = 3,
        DeadlineExceeded = 4,
        NotFound = 5,
        AlreadyExists = 6,
        PermissionDenied = 7,
        ResourceExhausted = 8,
        FailedPrecondition = 9,
        Aborted = 10,
        OutOfRange = 11,
        Unimplemented = 12,
        Internal = 13,
        Unavailable = 14,
        DataLoss = 15,
        Unauthenticated = 16,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcStatus
    {
        internal SpcStatusCode Code;
        internal IntPtr Message;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcStringView
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcStringViewSpan
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcIntSpan
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcBytes
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcIntArray
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcFloatArray
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcSizeArray
    {
        internal IntPtr Data;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcIntArrayList
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcBytesArray
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcBytesArrayList
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcScoredIntArray
    {
        internal SpcIntArray Ids;
        internal float Score;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcScoredIntArrayList
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcScoredBytesArray
    {
        internal SpcBytesArray Pieces;
        internal float Score;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcScoredBytesArrayList
    {
        internal IntPtr Items;
        internal nuint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcNormalizedResult
    {
        internal SpcBytes Normalized;
        internal SpcSizeArray Offsets;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcEncodeOptions
    {
        [MarshalAs(UnmanagedType.I1)] internal bool AddBos;
        [MarshalAs(UnmanagedType.I1)] internal bool AddEos;
        [MarshalAs(UnmanagedType.I1)] internal bool Reverse;
        [MarshalAs(UnmanagedType.I1)] internal bool EmitUnkPiece;
        [MarshalAs(UnmanagedType.I1)] internal bool EnableSampling;
        internal int NBestSize;
        internal float Alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcSampleEncodeAndScoreOptions
    {
        [MarshalAs(UnmanagedType.I1)] internal bool AddBos;
        [MarshalAs(UnmanagedType.I1)] internal bool AddEos;
        [MarshalAs(UnmanagedType.I1)] internal bool Reverse;
        [MarshalAs(UnmanagedType.I1)] internal bool EmitUnkPiece;
        internal int NumSamples;
        internal float Alpha;
        [MarshalAs(UnmanagedType.I1)] internal bool Wor;
        [MarshalAs(UnmanagedType.I1)] internal bool IncludeBest;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpcMapEntry
    {
        internal SpcStringView Key;
        internal SpcStringView Value;
    }

    internal sealed class ProcessorSafeHandle : SafeHandle
    {
        internal ProcessorSafeHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        internal void Initialize(IntPtr pointer) => SetHandle(pointer);

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            spc_sentencepiece_processor_destroy(handle);
            return true;
        }
    }

    internal sealed class NormalizerSafeHandle : SafeHandle
    {
        internal NormalizerSafeHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        internal void Initialize(IntPtr pointer) => SetHandle(pointer);

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            spc_sentencepiece_normalizer_destroy(handle);
            return true;
        }
    }

    [DllImport(LibraryName, EntryPoint = "spc_status_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_status_destroy(ref SpcStatus status);

    [DllImport(LibraryName, EntryPoint = "spc_bytes_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_bytes_destroy(ref SpcBytes value);

    [DllImport(LibraryName, EntryPoint = "spc_int_array_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_int_array_destroy(ref SpcIntArray value);

    [DllImport(LibraryName, EntryPoint = "spc_float_array_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_float_array_destroy(ref SpcFloatArray value);

    [DllImport(LibraryName, EntryPoint = "spc_size_array_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_size_array_destroy(ref SpcSizeArray value);

    [DllImport(LibraryName, EntryPoint = "spc_int_array_list_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_int_array_list_destroy(ref SpcIntArrayList value);

    [DllImport(LibraryName, EntryPoint = "spc_bytes_array_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_bytes_array_destroy(ref SpcBytesArray value);

    [DllImport(LibraryName, EntryPoint = "spc_bytes_array_list_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_bytes_array_list_destroy(ref SpcBytesArrayList value);

    [DllImport(LibraryName, EntryPoint = "spc_scored_int_array_list_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_scored_int_array_list_destroy(ref SpcScoredIntArrayList value);

    [DllImport(LibraryName, EntryPoint = "spc_scored_bytes_array_list_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_scored_bytes_array_list_destroy(ref SpcScoredBytesArrayList value);

    [DllImport(LibraryName, EntryPoint = "spc_normalized_result_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_normalized_result_destroy(ref SpcNormalizedResult value);

    [DllImport(LibraryName, EntryPoint = "spc_encode_options_init", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_encode_options_init(out SpcEncodeOptions options);

    [DllImport(LibraryName, EntryPoint = "spc_sample_encode_and_score_options_init", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_sample_encode_and_score_options_init(out SpcSampleEncodeAndScoreOptions options);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_create", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr spc_sentencepiece_processor_create();

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_sentencepiece_processor_destroy(IntPtr processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_load_from_file", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_load_from_file(ProcessorSafeHandle processor, SpcStringView filename);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_load_from_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_load_from_serialized_proto(ProcessorSafeHandle processor, IntPtr data, nuint length);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_set_encode_extra_options", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_set_encode_extra_options(ProcessorSafeHandle processor, SpcStringView extraOption);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_set_decode_extra_options", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_set_decode_extra_options(ProcessorSafeHandle processor, SpcStringView extraOption);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_set_vocabulary", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_set_vocabulary(ProcessorSafeHandle processor, IntPtr pieces, nuint length);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_reset_vocabulary", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_reset_vocabulary(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_load_vocabulary", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_load_vocabulary(ProcessorSafeHandle processor, SpcStringView filename, int threshold);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_get_piece_size", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_get_piece_size(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_piece_to_id", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_piece_to_id(ProcessorSafeHandle processor, SpcStringView piece);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_id_to_piece", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_id_to_piece(ProcessorSafeHandle processor, int id, out SpcBytes piece);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_get_score", CallingConvention = CallingConvention.Cdecl)]
    internal static extern float spc_sentencepiece_processor_get_score(ProcessorSafeHandle processor, int id);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_is_unknown", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool spc_sentencepiece_processor_is_unknown(ProcessorSafeHandle processor, int id);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_is_control", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool spc_sentencepiece_processor_is_control(ProcessorSafeHandle processor, int id);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_is_unused", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool spc_sentencepiece_processor_is_unused(ProcessorSafeHandle processor, int id);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_is_byte", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool spc_sentencepiece_processor_is_byte(ProcessorSafeHandle processor, int id);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_unk_id", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_unk_id(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_bos_id", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_bos_id(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_eos_id", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_eos_id(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_pad_id", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int spc_sentencepiece_processor_pad_id(ProcessorSafeHandle processor);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_serialized_model_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_serialized_model_proto(ProcessorSafeHandle processor, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_ids", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_ids(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcIntArray ids);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_pieces", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_pieces(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcBytesArray pieces);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_serialized_proto(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcBytes proto);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_ids_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_ids_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, IntPtr options, out SpcIntArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_pieces_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_pieces_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, IntPtr options, out SpcBytesArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_encode_serialized_proto_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_encode_serialized_proto_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, IntPtr options, out SpcBytesArray outputList);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids(ProcessorSafeHandle processor, IntPtr ids, nuint length, out SpcBytes text);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids_as_bytes", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids_as_bytes(ProcessorSafeHandle processor, IntPtr ids, nuint length, out SpcBytes bytes);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_pieces", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_pieces(ProcessorSafeHandle processor, IntPtr pieces, nuint length, out SpcBytes text);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids_as_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids_as_serialized_proto(ProcessorSafeHandle processor, IntPtr ids, nuint length, out SpcBytes proto);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_pieces_as_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_pieces_as_serialized_proto(ProcessorSafeHandle processor, IntPtr pieces, nuint length, out SpcBytes proto);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, out SpcBytesArray outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids_as_bytes_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids_as_bytes_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, out SpcBytesArray outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_ids_as_serialized_proto_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_ids_as_serialized_proto_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, out SpcBytesArray outputList);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_pieces_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_pieces_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, out SpcBytesArray outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_decode_pieces_as_serialized_proto_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_decode_pieces_as_serialized_proto_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, int numThreads, out SpcBytesArray outputList);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_nbest_encode_ids", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_nbest_encode_ids(ProcessorSafeHandle processor, SpcStringView input, int nbestSize, IntPtr options, out SpcIntArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_nbest_encode_pieces", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_nbest_encode_pieces(ProcessorSafeHandle processor, SpcStringView input, int nbestSize, IntPtr options, out SpcBytesArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_nbest_encode_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_nbest_encode_serialized_proto(ProcessorSafeHandle processor, SpcStringView input, int nbestSize, IntPtr options, out SpcBytes proto);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_sample_encode_and_score_ids", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_sample_encode_and_score_ids(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcScoredIntArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_sample_encode_and_score_pieces", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_sample_encode_and_score_pieces(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcScoredBytesArrayList outputs);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_sample_encode_and_score_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_sample_encode_and_score_serialized_proto(ProcessorSafeHandle processor, SpcStringView input, IntPtr options, out SpcBytes proto);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_normalize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_normalize(ProcessorSafeHandle processor, SpcStringView input, out SpcBytes text);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_normalize_with_offsets", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_normalize_with_offsets(ProcessorSafeHandle processor, SpcStringView input, out SpcNormalizedResult result);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_calculate_entropy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_calculate_entropy(ProcessorSafeHandle processor, SpcStringView input, float alpha, out float entropy);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_calculate_entropy_batch", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_calculate_entropy_batch(ProcessorSafeHandle processor, IntPtr inputs, nuint inputCount, float alpha, int numThreads, out SpcFloatArray entropies);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_processor_override_normalizer_spec", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_processor_override_normalizer_spec(ProcessorSafeHandle processor, IntPtr entries, nuint length);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_trainer_train_from_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_trainer_train_from_string(SpcStringView args, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_trainer_train_from_string_with_sentences", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_trainer_train_from_string_with_sentences(SpcStringView args, IntPtr sentences, nuint sentenceCount, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_trainer_train_from_map", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_trainer_train_from_map(IntPtr entries, nuint entryCount, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_trainer_train_from_map_with_sentences", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_trainer_train_from_map_with_sentences(IntPtr entries, nuint entryCount, IntPtr sentences, nuint sentenceCount, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_create", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr spc_sentencepiece_normalizer_create();

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_sentencepiece_normalizer_destroy(IntPtr normalizer);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_load_from_file", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_load_from_file(NormalizerSafeHandle normalizer, SpcStringView filename);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_load_from_serialized_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_load_from_serialized_proto(NormalizerSafeHandle normalizer, IntPtr data, nuint length);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_load_from_rule_tsv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_load_from_rule_tsv(NormalizerSafeHandle normalizer, SpcStringView filename);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_load_from_rule_name", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_load_from_rule_name(NormalizerSafeHandle normalizer, SpcStringView name);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_normalize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_normalize(NormalizerSafeHandle normalizer, SpcStringView input, out SpcBytes text);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_normalize_with_offsets", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_normalize_with_offsets(NormalizerSafeHandle normalizer, SpcStringView input, out SpcNormalizedResult result);

    [DllImport(LibraryName, EntryPoint = "spc_sentencepiece_normalizer_serialized_model_proto", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SpcStatus spc_sentencepiece_normalizer_serialized_model_proto(NormalizerSafeHandle normalizer, out SpcBytes model);

    [DllImport(LibraryName, EntryPoint = "spc_set_random_generator_seed", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_set_random_generator_seed(uint seed);

    [DllImport(LibraryName, EntryPoint = "spc_set_min_log_level", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_set_min_log_level(int level);

    [DllImport(LibraryName, EntryPoint = "spc_set_data_dir", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void spc_set_data_dir(SpcStringView dataDir);
}

