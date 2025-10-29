namespace ErgoX.TokenX.SentencePiece.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ErgoX.TokenX.SentencePiece.Exceptions;
using ErgoX.TokenX.SentencePiece.Internal.Interop;
using ErgoX.TokenX.SentencePiece.Models;
using ErgoX.TokenX.SentencePiece.Options;
using ErgoX.TokenX.HuggingFace.Tests;
using Xunit;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Filter, TestCategories.Unit)]
public sealed class InteropUtilitiesTests
{
    [Fact]
    public void EnsureSuccess_WhenStatusOk_DoesNotThrow()
    {
        var status = new NativeMethods.SpcStatus
        {
            Code = NativeMethods.SpcStatusCode.Ok,
            Message = IntPtr.Zero,
        };

        var exception = Record.Exception(() => InteropUtilities.EnsureSuccess(status));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureSuccess_WhenStatusFailsThrows()
    {
        var status = new NativeMethods.SpcStatus
        {
            Code = NativeMethods.SpcStatusCode.InvalidArgument,
            Message = IntPtr.Zero,
        };

        var exception = Assert.Throws<SentencePieceException>(() => InteropUtilities.EnsureSuccess(status));
        Assert.Contains(nameof(NativeMethods.SpcStatusCode.InvalidArgument), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AsStringView_StringHandlesNullAndContent()
    {
        var nullView = InteropUtilities.AsStringView((string?)null, out var nullUtf8);
        using (nullUtf8)
        {
            Assert.Equal(IntPtr.Zero, nullView.Data);
            Assert.Equal((nuint)0, nullView.Length);
        }

        var view = InteropUtilities.AsStringView("interop", out var utf8);
        try
        {
            Assert.NotEqual(IntPtr.Zero, view.Data);
            Assert.Equal((nuint)7, view.Length);
        }
        finally
        {
            utf8.Dispose();
        }
    }

    [Fact]
    public void AsStringView_SpanHandlesEmptyAndContent()
    {
        var emptyView = InteropUtilities.AsStringView(ReadOnlySpan<byte>.Empty, out var emptyBuffer);
        using (emptyBuffer)
        {
            Assert.Equal(IntPtr.Zero, emptyView.Data);
            Assert.Equal((nuint)0, emptyView.Length);
        }

        var payload = new byte[] { 1, 2, 3 };
        var view = InteropUtilities.AsStringView(payload.AsSpan(), out var buffer);
        try
        {
            Assert.NotEqual(IntPtr.Zero, view.Data);
            Assert.Equal((nuint)payload.Length, view.Length);
        }
        finally
        {
            buffer.Dispose();
        }
    }

    [Fact]
    public void CopyBytes_ReturnsManagedString()
    {
        var text = "native";
        var bytes = Encoding.UTF8.GetBytes(text);
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            var native = new NativeMethods.SpcBytes
            {
                Data = pointer,
                Length = (nuint)bytes.Length,
            };

            var result = InteropUtilities.CopyBytes(native);
            Assert.Equal(text, result);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void CopyBytesArray_ReadsEntries()
    {
        var inputs = new[] { "interop", "buffer" };
        var entrySize = Marshal.SizeOf<NativeMethods.SpcBytes>();
        var arrayPointer = Marshal.AllocHGlobal(entrySize * inputs.Length);
        var allocated = new List<IntPtr>();

        try
        {
            for (int i = 0; i < inputs.Length; ++i)
            {
                var bytes = Encoding.UTF8.GetBytes(inputs[i]);
                var dataPointer = Marshal.AllocHGlobal(bytes.Length);
                allocated.Add(dataPointer);
                Marshal.Copy(bytes, 0, dataPointer, bytes.Length);

                var native = new NativeMethods.SpcBytes
                {
                    Data = dataPointer,
                    Length = (nuint)bytes.Length,
                };

                Marshal.StructureToPtr(native, IntPtr.Add(arrayPointer, i * entrySize), fDeleteOld: false);
            }

            var array = new NativeMethods.SpcBytesArray
            {
                Items = arrayPointer,
                Length = (nuint)inputs.Length,
            };

            var result = InteropUtilities.CopyBytesArray(array);
            Assert.Equal(inputs, result);
        }
        finally
        {
            foreach (var pointer in allocated)
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pointer);
                }
            }

            Marshal.FreeHGlobal(arrayPointer);
        }
    }

    [Fact]
    public void CopyIntArray_ReturnsManagedCopy()
    {
        var values = new[] { 1, 2, 3 };
        var bytes = new byte[values.Length * sizeof(int)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        var pointer = Marshal.AllocHGlobal(bytes.Length);

        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            var native = new NativeMethods.SpcIntArray
            {
                Data = pointer,
                Length = (nuint)values.Length,
            };

            var result = InteropUtilities.CopyIntArray(native);
            Assert.Equal(values, result);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void CopySizeArray_ReturnsManagedSequence()
    {
        var values = new[] { 4L, 5L, 6L };
        var bytes = new byte[values.Length * sizeof(long)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        var pointer = Marshal.AllocHGlobal(bytes.Length);

        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            var array = new NativeMethods.SpcSizeArray
            {
                Data = pointer,
                Length = (nuint)values.Length,
            };

            var result = InteropUtilities.CopySizeArray(array);
            Assert.Equal(values.Select(value => (int)value), result);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void CreateEncodeOptions_AppliesValues()
    {
        var options = new EncodeOptions
        {
            AddBos = true,
            AddEos = true,
            Reverse = true,
            EmitUnknownPiece = true,
            EnableSampling = true,
            NBestSize = 8,
            Alpha = 0.7f,
        };

        var native = InteropUtilities.CreateEncodeOptions(options);
        Assert.True(native.AddBos);
        Assert.True(native.AddEos);
        Assert.True(native.Reverse);
        Assert.True(native.EmitUnkPiece);
        Assert.True(native.EnableSampling);
        Assert.Equal(options.NBestSize, native.NBestSize);
        Assert.Equal(options.Alpha, native.Alpha);
    }

    [Fact]
    public void CreateSampleOptions_AppliesValues()
    {
        var options = new SampleEncodeAndScoreOptions
        {
            AddBos = true,
            AddEos = true,
            Reverse = true,
            EmitUnknownPiece = true,
            NumSamples = 5,
            Alpha = 0.9f,
            WithoutReplacement = true,
            IncludeBest = true,
        };

        var native = InteropUtilities.CreateSampleOptions(options);
        Assert.True(native.AddBos);
        Assert.True(native.AddEos);
        Assert.True(native.Reverse);
        Assert.True(native.EmitUnkPiece);
        Assert.Equal(options.NumSamples, native.NumSamples);
        Assert.Equal(options.Alpha, native.Alpha);
        Assert.True(native.Wor);
        Assert.True(native.IncludeBest);
    }

    [Fact]
    public void NativeUtf8_CreatesNullTerminatedBuffer()
    {
        using var utf8 = new InteropUtilities.NativeUtf8("buffer");
        var view = utf8.View;
        Assert.NotEqual(IntPtr.Zero, view.Data);
        Assert.Equal((nuint)6, view.Length);
    }

    [Fact]
    public void NativeBuffer_ExposesViewForSpan()
    {
        using var buffer = new InteropUtilities.NativeBuffer(new byte[] { 7, 8, 9 });
        var view = buffer.View;
        Assert.NotEqual(IntPtr.Zero, view.Data);
        Assert.Equal((nuint)3, view.Length);
    }

    [Fact]
    public void NativeInt32Buffer_PinsValues()
    {
        using var buffer = new InteropUtilities.NativeInt32Buffer(new[] { 1, 2, 3 });
        Assert.NotEqual(IntPtr.Zero, buffer.Pointer);
        Assert.Equal((nuint)3, buffer.Length);
    }

    [Fact]
    public void NativeUtf8Array_PinsViews()
    {
        using var array = new InteropUtilities.NativeUtf8Array(new[] { "a", "b" });
        Assert.NotEqual(IntPtr.Zero, array.Pointer);
        Assert.Equal((nuint)2, array.Length);
    }

    [Fact]
    public void NativeIntSpanArray_BuildsSpans()
    {
        var sequences = new List<IReadOnlyList<int>>
        {
            new[] { 1, 2 },
            new[] { 3 },
        };

        using var array = new InteropUtilities.NativeIntSpanArray(sequences);
        Assert.NotEqual(IntPtr.Zero, array.Pointer);
        Assert.Equal((nuint)2, array.Length);
    }

    [Fact]
    public void NativeStringViewSpanArray_BuildsSpans()
    {
        var sequences = new List<IReadOnlyList<string>>
        {
            new[] { "hello" },
            new[] { "world", "!" },
        };

        using var array = new InteropUtilities.NativeStringViewSpanArray(sequences);
        Assert.NotEqual(IntPtr.Zero, array.Pointer);
        Assert.Equal((nuint)2, array.Length);
    }

    [Fact]
    public void NativeMapEntries_ExposesPinnedEntries()
    {
        var map = new Dictionary<string, string>
        {
            ["key"] = "value",
            ["other"] = "entry",
        };

        using var entries = new InteropUtilities.NativeMapEntries(map);
        Assert.NotEqual(IntPtr.Zero, entries.Pointer);
        Assert.Equal((nuint)2, entries.Length);
    }
}

