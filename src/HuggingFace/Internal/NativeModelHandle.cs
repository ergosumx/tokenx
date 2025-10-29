namespace ErgoX.TokenX.HuggingFace.Internal;

using System;
using System.Runtime.InteropServices;
using ErgoX.TokenX.HuggingFace.Internal.Interop;
using ErgoX.TokenX.HuggingFace.Options;

/// <summary>
/// Manages a native tokenizer model pointer obtained from the native tokenizers library.
/// </summary>
/// <remarks>
/// This handle wraps a native model instance (BPE, WordPiece, or Unigram) and ensures proper cleanup
/// via the <see cref="SafeHandle"/> pattern. Models can be created from JSON configurations,
/// individual files, or through factory methods for specific model types.
/// </remarks>
internal sealed class NativeModelHandle : SafeHandle
{
    private readonly INativeInterop _interop;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeModelHandle"/> class.
    /// </summary>
    /// <param name="interop">The native interop provider responsible for managing this handle.</param>
    private NativeModelHandle(INativeInterop interop)
        : base(IntPtr.Zero, ownsHandle: true)
    {
        _interop = interop;
    }

    /// <summary>
    /// Gets a value indicating whether this handle points to an invalid (null) native object.
    /// </summary>
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>
    /// Releases the native model resource.
    /// </summary>
    /// <returns>True if the handle was successfully released; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            _interop.TokenizersModelFree(handle);
        }

        return true;
    }

    /// <summary>
    /// Creates a model handle from JSON configuration.
    /// </summary>
    /// <param name="json">The JSON model configuration.</param>
    /// <param name="interop">The native interop provider for this handle.</param>
    /// <returns>A new handle wrapping the native model.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interop"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when model creation fails.</exception>
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

    /// <summary>
    /// Creates a BPE (Byte Pair Encoding) model handle from vocabulary and merges files.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary file.</param>
    /// <param name="mergesPath">Path to the merges file.</param>
    /// <param name="options">BPE model configuration options.</param>
    /// <param name="interop">The native interop provider for this handle.</param>
    /// <returns>A new handle wrapping the native BPE model.</returns>
    /// <exception cref="ArgumentException">Thrown when paths are null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options or interop are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dropout is not between 0 and 1.</exception>
    /// <exception cref="InvalidOperationException">Thrown when model creation fails.</exception>
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

    /// <summary>
    /// Creates a WordPiece model handle from a vocabulary file.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary file.</param>
    /// <param name="options">WordPiece model configuration options.</param>
    /// <param name="interop">The native interop provider for this handle.</param>
    /// <returns>A new handle wrapping the native WordPiece model.</returns>
    /// <exception cref="ArgumentException">Thrown when vocabulary path is null/empty or unknown token is not provided.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options or interop are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when MaxInputCharsPerWord is not greater than zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when model creation fails.</exception>
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

    /// <summary>
    /// Creates a Unigram model handle from a model file.
    /// </summary>
    /// <param name="modelPath">Path to the Unigram model file.</param>
    /// <param name="options">Unigram model configuration options.</param>
    /// <param name="interop">The native interop provider for this handle.</param>
    /// <returns>A new handle wrapping the native Unigram model.</returns>
    /// <exception cref="ArgumentException">Thrown when model path is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options or interop are null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when model creation fails.</exception>
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

    /// <summary>
    /// Invokes a callback with the native pointer, ensuring proper reference counting.
    /// </summary>
    /// <typeparam name="T">The return type of the callback.</typeparam>
    /// <param name="invoker">The callback to invoke with the native pointer.</param>
    /// <returns>The result returned by the callback.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invoker"/> is null.</exception>
    /// <remarks>
    /// This method safely exposes the underlying native pointer for interop calls
    /// while maintaining reference counting guarantees via DangerousAddRef and DangerousRelease.
    /// </remarks>
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

    /// <summary>
    /// Invokes a callback with the native pointer, ensuring proper reference counting.
    /// </summary>
    /// <param name="invoker">The callback to invoke with the native pointer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invoker"/> is null.</exception>
    /// <remarks>
    /// This method safely exposes the underlying native pointer for interop calls
    /// while maintaining reference counting guarantees via DangerousAddRef and DangerousRelease.
    /// </remarks>
    public void InvokeWithHandle(Action<IntPtr> invoker)
    {
        ArgumentNullException.ThrowIfNull(invoker);

        InvokeWithHandle(ptr =>
        {
            invoker(ptr);
            return true;
        });
    }

    /// <summary>
    /// Helper method to create a handle from a native pointer and status code.
    /// </summary>
    /// <param name="ptr">The native pointer.</param>
    /// <param name="status">The status code from the native call.</param>
    /// <param name="interop">The native interop provider.</param>
    /// <param name="fallbackMessage">Error message to use if native error message is unavailable.</param>
    /// <returns>A new handle wrapping the native pointer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when pointer is null or status indicates failure.</exception>
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

