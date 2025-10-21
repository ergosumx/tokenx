namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken.Internal;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken.Internal.Interop;

/// <summary>
/// Prepares unmanaged buffers required to instantiate <c>CoreBPE</c> via the native bridge.
/// </summary>
internal sealed unsafe class CoreBpeArguments : IDisposable
{
    private readonly void* mergeBytesBuffer;
    private readonly NativeMethods.NativeMergeEntry* mergesPointer;
    private readonly NativeMethods.NativeSpecialToken* specialsPointer;
    private readonly List<IntPtr> specialTokenStrings = new();
    private bool disposed;

    internal CoreBpeArguments(
        IReadOnlyList<TiktokenMergeableRank> mergeableRanks,
        IReadOnlyDictionary<string, int> specialTokens,
        uint explicitVocabularySize)
    {
        ExplicitVocabularySize = explicitVocabularySize;

        if (mergeableRanks is null)
        {
            throw new ArgumentNullException(nameof(mergeableRanks));
        }

        if (specialTokens is null)
        {
            throw new ArgumentNullException(nameof(specialTokens));
        }

        MergesLength = (nuint)mergeableRanks.Count;
        SpecialsLength = (nuint)specialTokens.Count;

        if (mergeableRanks.Count > 0)
        {
            var totalByteLength = CalculateTotalByteLength(mergeableRanks);
            mergeBytesBuffer = totalByteLength == 0 ? null : NativeMemory.Alloc(totalByteLength);
            mergesPointer = (NativeMethods.NativeMergeEntry*)NativeMemory.Alloc(
                (nuint)mergeableRanks.Count,
                (nuint)Unsafe.SizeOf<NativeMethods.NativeMergeEntry>());

            FillMergeEntries(mergeableRanks, mergeBytesBuffer, mergesPointer);
        }

        if (specialTokens.Count > 0)
        {
            specialsPointer = (NativeMethods.NativeSpecialToken*)NativeMemory.Alloc(
                (nuint)specialTokens.Count,
                (nuint)Unsafe.SizeOf<NativeMethods.NativeSpecialToken>());

            FillSpecialTokens(specialTokens, specialsPointer);
        }
    }

    internal NativeMethods.NativeMergeEntry* MergesPointer => mergesPointer;

    internal nuint MergesLength { get; }

    internal NativeMethods.NativeSpecialToken* SpecialsPointer => specialsPointer;

    internal nuint SpecialsLength { get; }

    internal uint ExplicitVocabularySize { get; }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (mergesPointer != null)
        {
            NativeMemory.Free(mergesPointer);
        }

        if (mergeBytesBuffer != null)
        {
            NativeMemory.Free(mergeBytesBuffer);
        }

        if (specialsPointer != null)
        {
            NativeMemory.Free(specialsPointer);
        }

        foreach (var ptr in specialTokenStrings)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        specialTokenStrings.Clear();
        GC.SuppressFinalize(this);
    }

    ~CoreBpeArguments()
    {
        Dispose();
    }

    private static nuint CalculateTotalByteLength(IReadOnlyList<TiktokenMergeableRank> mergeableRanks)
    {
        nuint total = 0;
        foreach (var rank in mergeableRanks)
        {
            total += (nuint)rank.Token.Length;
        }

        return total;
    }

    private static void FillMergeEntries(
        IReadOnlyList<TiktokenMergeableRank> mergeableRanks,
        void* byteBuffer,
        NativeMethods.NativeMergeEntry* entries)
    {
        var cursor = (byte*)byteBuffer;

        for (var index = 0; index < mergeableRanks.Count; index++)
        {
            var tokenBytes = mergeableRanks[index].Token.Span;
            var length = (nuint)tokenBytes.Length;

            if (length > 0)
            {
                var destination = new Span<byte>(cursor, (int)length);
                tokenBytes.CopyTo(destination);
            }

            entries[index] = new NativeMethods.NativeMergeEntry(
                new NativeMethods.NativeBytes((IntPtr)cursor, length),
                (uint)mergeableRanks[index].Rank);

            cursor += length;
        }
    }

    private void FillSpecialTokens(
        IReadOnlyDictionary<string, int> specialTokens,
        NativeMethods.NativeSpecialToken* entries)
    {
        var index = 0;
        foreach (var kvp in specialTokens)
        {
            if (kvp.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(specialTokens),
                    $"Special token '{kvp.Key}' must map to a non-negative id.");
            }

            var ptr = Marshal.StringToCoTaskMemUTF8(kvp.Key);
            specialTokenStrings.Add(ptr);
            entries[index] = new NativeMethods.NativeSpecialToken(ptr, (uint)kvp.Value);
            index++;
        }
    }
}
