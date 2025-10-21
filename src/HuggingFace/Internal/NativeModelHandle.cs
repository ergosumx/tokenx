namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;

using System;
using System.Runtime.InteropServices;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

internal sealed class NativeModelHandle : SafeHandle
{
    private readonly INativeInterop _interop;

    private NativeModelHandle(INativeInterop interop)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        _interop = interop;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            _interop.TokenizersModelFree(handle);
        }

        return true;
    }

    public static NativeModelHandle Create(string json, INativeInterop interop)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Model JSON must be provided.", nameof(json));
        }

        ArgumentNullException.ThrowIfNull(interop);

        var ptr = interop.TokenizersModelFromJson(json, out var status);
        return CreateFromPointer(ptr, status, interop, "Failed to create tokenizer model.");
    }

    public static NativeModelHandle CreateBpe(string vocabPath, string mergesPath, BpeModelOptions options, INativeInterop interop)
    {
        if (string.IsNullOrWhiteSpace(vocabPath))
        {
            throw new ArgumentException("Vocabulary path must be provided.", nameof(vocabPath));
        }

        if (string.IsNullOrWhiteSpace(mergesPath))
        {
            throw new ArgumentException("Merges path must be provided.", nameof(mergesPath));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(interop);

        if (options.Dropout is { } dropout && (dropout < 0f || dropout > 1f))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Dropout must be between 0 and 1 (inclusive).");
        }

    var request = new NativeBpeModelParameters(vocabPath, mergesPath)
        {
            Dropout = options.Dropout ?? 0f,
            HasDropout = options.Dropout.HasValue,
            UnknownToken = options.UnknownToken,
            ContinuingSubwordPrefix = options.ContinuingSubwordPrefix,
            EndOfWordSuffix = options.EndOfWordSuffix,
            FuseUnknown = options.FuseUnknownTokens,
            EnableByteFallback = options.EnableByteFallback
        };

        var ptr = interop.TokenizersModelBpeFromFiles(in request, out var status);
        return CreateFromPointer(ptr, status, interop, "Failed to create BPE tokenizer model.");
    }

    public static NativeModelHandle CreateWordPiece(string vocabPath, WordPieceModelOptions options, INativeInterop interop)
    {
        if (string.IsNullOrWhiteSpace(vocabPath))
        {
            throw new ArgumentException("Vocabulary path must be provided.", nameof(vocabPath));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(interop);

        var unkToken = options.UnknownToken;
        if (string.IsNullOrWhiteSpace(unkToken))
        {
            throw new ArgumentException("Unknown token must be provided.", nameof(options));
        }

        if (options.MaxInputCharsPerWord is { } maxChars && maxChars <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxInputCharsPerWord must be greater than zero.");
        }

        var ptr = interop.TokenizersModelWordPieceFromFile(
            vocabPath,
            unkToken,
            options.ContinuingSubwordPrefix,
            options.MaxInputCharsPerWord.HasValue ? (nuint)options.MaxInputCharsPerWord.Value : 0,
            options.MaxInputCharsPerWord.HasValue,
            out var status);

        return CreateFromPointer(ptr, status, interop, "Failed to create WordPiece tokenizer model.");
    }

    public static NativeModelHandle CreateUnigram(string modelPath, UnigramModelOptions options, INativeInterop interop)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            throw new ArgumentException("Model path must be provided.", nameof(modelPath));
        }

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(interop);

        var ptr = interop.TokenizersModelUnigramFromFile(modelPath, out var status);
        return CreateFromPointer(ptr, status, interop, "Failed to create Unigram tokenizer model.");
    }

    public T InvokeWithHandle<T>(Func<IntPtr, T> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        var addedRef = false;
        try
        {
            DangerousAddRef(ref addedRef);
            return invoker(handle);
        }
        finally
        {
            if (addedRef)
            {
                DangerousRelease();
            }
        }
    }

    public void InvokeWithHandle(Action<IntPtr> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        InvokeWithHandle(ptr =>
        {
            invoker(ptr);
            return true;
        });
    }

    private static NativeModelHandle CreateFromPointer(IntPtr ptr, int status, INativeInterop interop, string fallbackMessage)
    {
        if (ptr == IntPtr.Zero || status != 0)
        {
            var message = interop.GetLastErrorMessage() ?? fallbackMessage;
            throw new InvalidOperationException(message);
        }

        var handle = new NativeModelHandle(interop);
        handle.SetHandle(ptr);
        return handle;
    }
}
