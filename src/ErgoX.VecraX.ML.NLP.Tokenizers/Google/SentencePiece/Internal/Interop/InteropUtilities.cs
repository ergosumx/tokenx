namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Exceptions;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Models;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Options;

/// <summary>
/// Provides utility methods for interoperability between managed C# code and the native SentencePiece C API.
/// Handles marshaling of strings, arrays, and complex structures between managed and unmanaged memory.
/// </summary>
internal static class InteropUtilities
{
    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly int SpcBytesSize = Marshal.SizeOf<NativeMethods.SpcBytes>();
    private static readonly int SpcIntArraySize = Marshal.SizeOf<NativeMethods.SpcIntArray>();
    private static readonly int SpcBytesArraySize = Marshal.SizeOf<NativeMethods.SpcBytesArray>();
    private static readonly int SpcScoredIntArraySize = Marshal.SizeOf<NativeMethods.SpcScoredIntArray>();
    private static readonly int SpcScoredBytesArraySize = Marshal.SizeOf<NativeMethods.SpcScoredBytesArray>();

    internal static void EnsureSuccess(NativeMethods.SpcStatus status)
    {
        var mutable = status;
        string? message = null;
        try
        {
            if (mutable.Code != NativeMethods.SpcStatusCode.Ok)
            {
                var statusCode = SentencePieceException.FromNative(mutable.Code);
                if (mutable.Message != IntPtr.Zero)
                {
                    message = Marshal.PtrToStringUTF8(mutable.Message);
                }

                message ??= statusCode.ToString();
                throw new SentencePieceException(message, statusCode);
            }
        }
        finally
        {
            NativeMethods.spc_status_destroy(ref mutable);
        }
    }

    internal static NativeMethods.SpcStringView AsStringView(string? value, out NativeUtf8 utf8)
    {
        utf8 = new NativeUtf8(value);
        return utf8.View;
    }

    internal static NativeMethods.SpcStringView AsStringView(ReadOnlySpan<byte> value, out NativeBuffer buffer)
    {
        buffer = new NativeBuffer(value);
        return buffer.View;
    }

    internal static string BytesToStringAndDestroy(ref NativeMethods.SpcBytes bytes)
    {
        try
        {
            if (bytes.Data == IntPtr.Zero || bytes.Length == 0)
            {
                return string.Empty;
            }

            var data = new byte[(int)bytes.Length];
            Marshal.Copy(bytes.Data, data, 0, data.Length);
            return Utf8.GetString(data);
        }
        finally
        {
            NativeMethods.spc_bytes_destroy(ref bytes);
        }
    }

    internal static byte[] BytesToArrayAndDestroy(ref NativeMethods.SpcBytes bytes)
    {
        try
        {
            if (bytes.Data == IntPtr.Zero || bytes.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var data = new byte[(int)bytes.Length];
            Marshal.Copy(bytes.Data, data, 0, data.Length);
            return data;
        }
        finally
        {
            NativeMethods.spc_bytes_destroy(ref bytes);
        }
    }

    internal static int[] IntArrayToManagedAndDestroy(ref NativeMethods.SpcIntArray array)
    {
        try
        {
            if (array.Data == IntPtr.Zero || array.Length == 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[(int)array.Length];
            Marshal.Copy(array.Data, result, 0, result.Length);
            return result;
        }
        finally
        {
            NativeMethods.spc_int_array_destroy(ref array);
        }
    }

    internal static IReadOnlyList<int[]> IntArrayListToManagedAndDestroy(ref NativeMethods.SpcIntArrayList list)
    {
        try
        {
            if (list.Length == 0 || list.Items == IntPtr.Zero)
            {
                return Array.Empty<int[]>();
            }

            var result = new int[(int)list.Length][];
            var current = list.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcIntArray>(current);
                result[i] = CopyIntArray(entry);
                current = IntPtr.Add(current, SpcIntArraySize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_int_array_list_destroy(ref list);
        }
    }

    internal static IReadOnlyList<string> BytesArrayToManagedAndDestroy(ref NativeMethods.SpcBytesArray array)
    {
        try
        {
            if (array.Length == 0 || array.Items == IntPtr.Zero)
            {
                return Array.Empty<string>();
            }

            var result = new string[(int)array.Length];
            var current = array.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcBytes>(current);
                result[i] = CopyBytes(entry);
                current = IntPtr.Add(current, SpcBytesSize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_bytes_array_destroy(ref array);
        }
    }

    internal static IReadOnlyList<IReadOnlyList<string>> BytesArrayListToManagedAndDestroy(ref NativeMethods.SpcBytesArrayList list)
    {
        try
        {
            if (list.Length == 0 || list.Items == IntPtr.Zero)
            {
                return Array.Empty<IReadOnlyList<string>>();
            }

            var result = new IReadOnlyList<string>[(int)list.Length];
            var current = list.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcBytesArray>(current);
                result[i] = CopyBytesArray(entry);
                current = IntPtr.Add(current, SpcBytesArraySize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_bytes_array_list_destroy(ref list);
        }
    }

    internal static IReadOnlyList<ScoredIds> ScoredIntArrayListToManagedAndDestroy(ref NativeMethods.SpcScoredIntArrayList list)
    {
        try
        {
            if (list.Length == 0 || list.Items == IntPtr.Zero)
            {
                return Array.Empty<ScoredIds>();
            }

            var result = new ScoredIds[(int)list.Length];
            var current = list.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcScoredIntArray>(current);
                result[i] = new ScoredIds(CopyIntArray(entry.Ids), entry.Score);
                current = IntPtr.Add(current, SpcScoredIntArraySize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_scored_int_array_list_destroy(ref list);
        }
    }

    internal static IReadOnlyList<ScoredPieces> ScoredBytesArrayListToManagedAndDestroy(ref NativeMethods.SpcScoredBytesArrayList list)
    {
        try
        {
            if (list.Length == 0 || list.Items == IntPtr.Zero)
            {
                return Array.Empty<ScoredPieces>();
            }

            var result = new ScoredPieces[(int)list.Length];
            var current = list.Items;
            for (int i = 0; i < result.Length; ++i)
            {
                var entry = Marshal.PtrToStructure<NativeMethods.SpcScoredBytesArray>(current);
                result[i] = new ScoredPieces(CopyBytesArray(entry.Pieces), entry.Score);
                current = IntPtr.Add(current, SpcScoredBytesArraySize);
            }

            return result;
        }
        finally
        {
            NativeMethods.spc_scored_bytes_array_list_destroy(ref list);
        }
    }

    internal static NormalizedResult NormalizedResultToManagedAndDestroy(ref NativeMethods.SpcNormalizedResult result)
    {
        try
        {
            var text = CopyBytes(result.Normalized);
            var offsets = CopySizeArray(result.Offsets);
            return new NormalizedResult(text, offsets);
        }
        finally
        {
            NativeMethods.spc_normalized_result_destroy(ref result);
        }
    }

    internal static IReadOnlyList<float> FloatArrayToManagedAndDestroy(ref NativeMethods.SpcFloatArray array)
    {
        try
        {
            if (array.Data == IntPtr.Zero || array.Length == 0)
            {
                return Array.Empty<float>();
            }

            var result = new float[(int)array.Length];
            Marshal.Copy(array.Data, result, 0, result.Length);
            return result;
        }
        finally
        {
            NativeMethods.spc_float_array_destroy(ref array);
        }
    }

    internal static IReadOnlyList<int> CopySizeArray(NativeMethods.SpcSizeArray array)
    {
        if (array.Data == IntPtr.Zero || array.Length == 0)
        {
            return Array.Empty<int>();
        }

        var result = new int[(int)array.Length];
        if (Environment.Is64BitProcess)
        {
            var buffer = new long[result.Length];
            Marshal.Copy(array.Data, buffer, 0, buffer.Length);
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = checked((int)buffer[i]);
            }
        }
        else
        {
            var buffer = new int[result.Length];
            Marshal.Copy(array.Data, buffer, 0, buffer.Length);
            Array.Copy(buffer, result, result.Length);
        }

        return result;
    }

    internal static int[] CopyIntArray(NativeMethods.SpcIntArray array)
    {
        if (array.Data == IntPtr.Zero || array.Length == 0)
        {
            return Array.Empty<int>();
        }

        var result = new int[(int)array.Length];
        Marshal.Copy(array.Data, result, 0, result.Length);
        return result;
    }

    internal static string CopyBytes(NativeMethods.SpcBytes bytes)
    {
        if (bytes.Data == IntPtr.Zero || bytes.Length == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[(int)bytes.Length];
        Marshal.Copy(bytes.Data, buffer, 0, buffer.Length);
        return Utf8.GetString(buffer);
    }

    internal static IReadOnlyList<string> CopyBytesArray(NativeMethods.SpcBytesArray array)
    {
        if (array.Length == 0 || array.Items == IntPtr.Zero)
        {
            return Array.Empty<string>();
        }

        var result = new string[(int)array.Length];
        var current = array.Items;
        for (int i = 0; i < result.Length; ++i)
        {
            var entry = Marshal.PtrToStructure<NativeMethods.SpcBytes>(current);
            result[i] = CopyBytes(entry);
            current = IntPtr.Add(current, SpcBytesSize);
        }

        return result;
    }

    internal static NativeMethods.SpcEncodeOptions CreateEncodeOptions(EncodeOptions? options)
    {
        NativeMethods.spc_encode_options_init(out var native);
        if (options is null)
        {
            return native;
        }

        native.AddBos = options.AddBos;
        native.AddEos = options.AddEos;
        native.Reverse = options.Reverse;
        native.EmitUnkPiece = options.EmitUnknownPiece;
        native.EnableSampling = options.EnableSampling;
        native.NBestSize = options.NBestSize;
        native.Alpha = options.Alpha;
        return native;
    }

    internal static NativeMethods.SpcSampleEncodeAndScoreOptions CreateSampleOptions(SampleEncodeAndScoreOptions? options)
    {
        NativeMethods.spc_sample_encode_and_score_options_init(out var native);
        if (options is null)
        {
            return native;
        }

        native.AddBos = options.AddBos;
        native.AddEos = options.AddEos;
        native.Reverse = options.Reverse;
        native.EmitUnkPiece = options.EmitUnknownPiece;
        native.NumSamples = options.NumSamples;
        native.Alpha = options.Alpha;
        native.Wor = options.WithoutReplacement;
        native.IncludeBest = options.IncludeBest;
        return native;
    }

    internal readonly struct ScoredIds
    {
        internal ScoredIds(IReadOnlyList<int> ids, float score)
        {
            Ids = ids;
            Score = score;
        }

        internal IReadOnlyList<int> Ids { get; }

        internal float Score { get; }
    }

    internal readonly struct ScoredPieces
    {
        internal ScoredPieces(IReadOnlyList<string> pieces, float score)
        {
            Pieces = pieces;
            Score = score;
        }

        internal IReadOnlyList<string> Pieces { get; }

        internal float Score { get; }
    }

    internal readonly struct NormalizedResult
    {
        internal NormalizedResult(string text, IReadOnlyList<int> offsets)
        {
            Text = text;
            Offsets = offsets;
        }

        internal string Text { get; }

        internal IReadOnlyList<int> Offsets { get; }
    }

    internal sealed class NativeUtf8 : IDisposable
    {
        private readonly IntPtr pointer;
        private readonly nuint length;

        internal NativeUtf8(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                pointer = IntPtr.Zero;
                length = 0;
                return;
            }

            var bytes = Utf8.GetBytes(value);
            pointer = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            Marshal.WriteByte(pointer, bytes.Length, 0);
            length = (nuint)bytes.Length;
        }

        internal NativeMethods.SpcStringView View => new() { Data = pointer, Length = length };

        public void Dispose()
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }

    internal sealed class NativeBuffer : IDisposable
    {
        private readonly IntPtr pointer;
        private readonly nuint length;

        internal NativeBuffer(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
            {
                pointer = IntPtr.Zero;
                length = 0;
                return;
            }

            pointer = Marshal.AllocHGlobal(value.Length);
            Marshal.Copy(value.ToArray(), 0, pointer, value.Length);
            length = (nuint)value.Length;
        }

        internal NativeMethods.SpcStringView View => new() { Data = pointer, Length = length };

        public void Dispose()
        {
            if (pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }

    internal sealed class NativeUtf8Array : IDisposable
    {
        private readonly NativeUtf8[] buffers;
        private readonly NativeMethods.SpcStringView[] views;
        private GCHandle handle;

        internal NativeUtf8Array(IEnumerable<string> values)
        {
            buffers = (values ?? Array.Empty<string>()).Select(value => new NativeUtf8(value)).ToArray();
            views = new NativeMethods.SpcStringView[buffers.Length];
            for (int i = 0; i < buffers.Length; ++i)
            {
                views[i] = buffers[i].View;
            }

            handle = views.Length == 0 ? default : GCHandle.Alloc(views, GCHandleType.Pinned);
        }

        internal IntPtr Pointer => views.Length == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

        internal nuint Length => (nuint)views.Length;

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }

            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }
    }

    internal sealed class NativeInt32Buffer : IDisposable
    {
        private readonly int[] managed;
        private readonly GCHandle handle;

        internal NativeInt32Buffer(IEnumerable<int> values)
        {
            managed = values?.ToArray() ?? Array.Empty<int>();
            handle = managed.Length == 0 ? default : GCHandle.Alloc(managed, GCHandleType.Pinned);
        }

        internal IntPtr Pointer => managed.Length == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

        internal nuint Length => (nuint)managed.Length;

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }

    internal sealed class NativeIntSpanArray : IDisposable
    {
        private readonly NativeInt32Buffer[] buffers;
        private readonly NativeMethods.SpcIntSpan[] spans;
        private GCHandle handle;

        internal NativeIntSpanArray(IEnumerable<IReadOnlyList<int>> sequences)
        {
            var list = sequences?.ToList() ?? new List<IReadOnlyList<int>>();
            buffers = new NativeInt32Buffer[list.Count];
            spans = new NativeMethods.SpcIntSpan[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                buffers[i] = new NativeInt32Buffer(list[i]);
                spans[i] = new NativeMethods.SpcIntSpan
                {
                    Data = buffers[i].Pointer,
                    Length = buffers[i].Length,
                };
            }

            handle = spans.Length == 0 ? default : GCHandle.Alloc(spans, GCHandleType.Pinned);
        }

        internal IntPtr Pointer => spans.Length == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

        internal nuint Length => (nuint)spans.Length;

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }

            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }
    }

    internal sealed class NativeStringViewSpanArray : IDisposable
    {
        private readonly NativeUtf8Array[] arrays;
        private readonly NativeMethods.SpcStringViewSpan[] spans;
        private GCHandle handle;

        internal NativeStringViewSpanArray(IEnumerable<IReadOnlyList<string>> sequences)
        {
            var list = sequences?.ToList() ?? new List<IReadOnlyList<string>>();
            arrays = new NativeUtf8Array[list.Count];
            spans = new NativeMethods.SpcStringViewSpan[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                arrays[i] = new NativeUtf8Array(list[i]);
                spans[i] = new NativeMethods.SpcStringViewSpan
                {
                    Items = arrays[i].Pointer,
                    Length = arrays[i].Length,
                };
            }

            handle = spans.Length == 0 ? default : GCHandle.Alloc(spans, GCHandleType.Pinned);
        }

        internal IntPtr Pointer => spans.Length == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

        internal nuint Length => (nuint)spans.Length;

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }

            foreach (var array in arrays)
            {
                array.Dispose();
            }
        }
    }

    internal sealed class NativeMapEntries : IDisposable
    {
        private readonly NativeUtf8[] keys;
        private readonly NativeUtf8[] values;
        private readonly NativeMethods.SpcMapEntry[] entries;
        private GCHandle handle;

        internal NativeMapEntries(IEnumerable<KeyValuePair<string, string>> map)
        {
            var list = map?.ToList() ?? new List<KeyValuePair<string, string>>();
            keys = new NativeUtf8[list.Count];
            values = new NativeUtf8[list.Count];
            entries = new NativeMethods.SpcMapEntry[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                keys[i] = new NativeUtf8(list[i].Key);
                values[i] = new NativeUtf8(list[i].Value);
                entries[i] = new NativeMethods.SpcMapEntry
                {
                    Key = keys[i].View,
                    Value = values[i].View,
                };
            }

            handle = entries.Length == 0 ? default : GCHandle.Alloc(entries, GCHandleType.Pinned);
        }

        internal IntPtr Pointer => entries.Length == 0 ? IntPtr.Zero : handle.AddrOfPinnedObject();

        internal nuint Length => (nuint)entries.Length;

        public void Dispose()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }

            foreach (var key in keys)
            {
                key.Dispose();
            }

            foreach (var value in values)
            {
                value.Dispose();
            }
        }
    }
}
