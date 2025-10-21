namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken.Internal;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken.Internal.Interop;

/// <summary>
/// Managed facade for the TikToken <c>CoreBPE</c> implementation.
/// </summary>
public sealed class TiktokenEncoding : IDisposable
{
    private readonly object syncRoot = new();
    private readonly ReadOnlyDictionary<string, int> specialTokens;
    private IntPtr handle;
    private bool disposed;

    private TiktokenEncoding(string name, string pattern, IntPtr handle, IReadOnlyDictionary<string, int> specialTokens)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        this.handle = handle;
        this.specialTokens = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(specialTokens));
    }

    public string Name { get; }

    public string Pattern { get; }

    public IReadOnlyDictionary<string, int> SpecialTokens => specialTokens;

    public static TiktokenEncoding Create(
        string name,
        string pattern,
        IReadOnlyList<TiktokenMergeableRank> mergeableRanks,
        IReadOnlyDictionary<string, int> specialTokens,
        uint? explicitVocabularySize = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Encoding name must be provided.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern must be provided.", nameof(pattern));
        }

        if (mergeableRanks is null)
        {
            throw new ArgumentNullException(nameof(mergeableRanks));
        }

        if (specialTokens is null)
        {
            throw new ArgumentNullException(nameof(specialTokens));
        }

        explicitVocabularySize ??= CalculateExplicitVocabularySize(mergeableRanks, specialTokens);

        using var arguments = new CoreBpeArguments(mergeableRanks, specialTokens, explicitVocabularySize.GetValueOrDefault());
        IntPtr ptr;
        unsafe
        {
            ptr = NativeMethods.CoreBpeNew(
                pattern,
                arguments.MergesPointer,
                arguments.MergesLength,
                arguments.SpecialsPointer,
                arguments.SpecialsLength,
                arguments.ExplicitVocabularySize);
        }

        if (ptr == IntPtr.Zero)
        {
            ThrowLastInteropError("Failed to create TikToken encoder.");
        }

        return new TiktokenEncoding(name, pattern, ptr, specialTokens);
    }

    public IReadOnlyList<uint> EncodeOrdinary(string text)
    {
        EnsureNotDisposed();
        unsafe
        {
            var encoding = NativeMethods.CoreBpeEncodeOrdinary(handle, text ?? string.Empty);
            return ExtractTokens(encoding);
        }
    }

    public IReadOnlyList<uint> Encode(string text, IReadOnlyCollection<string>? allowedSpecial = null)
    {
        EnsureNotDisposed();

        if (allowedSpecial is null || allowedSpecial.Count == 0)
        {
            unsafe
            {
                var encoding = NativeMethods.CoreBpeEncode(handle, text ?? string.Empty, null, 0);
                return ExtractTokens(encoding);
            }
        }

        var handles = new List<IntPtr>(allowedSpecial.Count);
        var buffer = new IntPtr[allowedSpecial.Count];
        var index = 0;

        try
        {
            foreach (var token in allowedSpecial)
            {
                if (token is null)
                {
                    throw new ArgumentException("Allowed special tokens cannot contain null entries.", nameof(allowedSpecial));
                }

                var ptr = Marshal.StringToCoTaskMemUTF8(token);
                handles.Add(ptr);
                buffer[index++] = ptr;
            }

            unsafe
            {
                fixed (IntPtr* ptr = buffer)
                {
                    var encoding = NativeMethods.CoreBpeEncode(handle, text ?? string.Empty, ptr, (nuint)buffer.Length);
                    return ExtractTokens(encoding);
                }
            }
        }
        finally
        {
            foreach (var allocated in handles)
            {
                if (allocated != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(allocated);
                }
            }
        }
    }

    public string Decode(ReadOnlySpan<uint> tokens)
    {
        EnsureNotDisposed();

        unsafe
        {
            fixed (uint* ptr = tokens)
            {
                var handlePtr = NativeMethods.CoreBpeDecode(handle, ptr, (nuint)tokens.Length);
                return ExtractString(handlePtr, Encoding.UTF8);
            }
        }
    }

    public byte[] DecodeBytes(ReadOnlySpan<uint> tokens)
    {
        EnsureNotDisposed();

        unsafe
        {
            fixed (uint* ptr = tokens)
            {
                var stringPtr = NativeMethods.CoreBpeDecodeBytes(handle, ptr, (nuint)tokens.Length);
                return ExtractBytes(stringPtr);
            }
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        lock (syncRoot)
        {
            if (disposed)
            {
                return;
            }

            if (handle != IntPtr.Zero)
            {
                NativeMethods.CoreBpeFree(handle);
                handle = IntPtr.Zero;
            }

            disposed = true;
        }
    }

    ~TiktokenEncoding()
    {
        Dispose();
    }

    private static uint? CalculateExplicitVocabularySize(
        IReadOnlyList<TiktokenMergeableRank> mergeableRanks,
        IReadOnlyDictionary<string, int> specialTokens)
    {
        if (mergeableRanks.Count == 0 && specialTokens.Count == 0)
        {
            return null;
        }

        var maxRank = mergeableRanks.Count > 0 ? mergeableRanks.Max(r => r.Rank) : 0;
        if (specialTokens.Count > 0)
        {
            maxRank = Math.Max(maxRank, specialTokens.Max(kvp => kvp.Value));
        }

        return (uint)(maxRank + 1);
    }

    private void EnsureNotDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(TiktokenEncoding));
        }
    }

    private static IReadOnlyList<uint> ExtractTokens(IntPtr encoding)
    {
        if (encoding == IntPtr.Zero)
        {
            ThrowLastInteropError("Failed to encode text.");
        }

        try
        {
            var length = (int)NativeMethods.EncodingGetLength(encoding);
            if (length == 0)
            {
                return Array.Empty<uint>();
            }

            var buffer = new uint[length];

            unsafe
            {
                fixed (uint* destination = buffer)
                {
                    if (!NativeMethods.EncodingTryCopyTokens(encoding, destination, (nuint)length))
                    {
                        throw new TiktokenInteropException("Native encoding buffer copy failed.");
                    }
                }
            }

            return buffer;
        }
        finally
        {
            NativeMethods.EncodingFree(encoding);
        }
    }

    private static string ExtractString(IntPtr nativeString, Encoding encoding)
    {
        if (nativeString == IntPtr.Zero)
        {
            ThrowLastInteropError("Failed to decode tokens into text.");
        }

        var bytes = ExtractBytes(nativeString);
        return encoding.GetString(bytes);
    }

    private static byte[] ExtractBytes(IntPtr nativeString)
    {
        if (nativeString == IntPtr.Zero)
        {
            ThrowLastInteropError("Failed to decode tokens.");
        }

        try
        {
            var length = (int)NativeMethods.StringGetLength(nativeString);
            if (length == 0)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[length];

            unsafe
            {
                fixed (byte* destination = buffer)
                {
                    if (!NativeMethods.StringTryCopyBytes(nativeString, destination, (nuint)length))
                    {
                        throw new TiktokenInteropException("Native string buffer copy failed.");
                    }
                }
            }

            return buffer;
        }
        finally
        {
            NativeMethods.StringFree(nativeString);
        }
    }

    private static void ThrowLastInteropError(string fallback)
    {
        var message = NativeMethods.ConsumeLastError() ?? fallback;
        throw new TiktokenInteropException(message);
    }
}
