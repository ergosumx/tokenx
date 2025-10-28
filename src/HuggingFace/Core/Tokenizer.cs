using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Abstractions;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

/// <summary>
/// A wrapper around the tokenizers library providing text encoding/decoding and chat template rendering.
/// </summary>
/// <remarks>
/// This class provides thread-safe access to the native tokenizer implementation,
/// supporting BPE, WordPiece, and Unigram models. It handles encoding with padding/truncation,
/// batch processing, and HuggingFace chat template rendering.
/// </remarks>
public sealed class Tokenizer : ITokenizer
{
    private readonly NativeTokenizerHandle _handle;
    private readonly INativeInterop _interop;
    private readonly object _syncRoot = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private Tokenizer(NativeTokenizerHandle handle, INativeInterop interop)
    {
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentNullException.ThrowIfNull(interop);
        _handle = handle;
        _interop = interop;
    }

    public Tokenizer(string jsonConfig)
        : this(CreateHandleFromJson(jsonConfig, out var interop), interop)
    {
    }

    public static Tokenizer FromPretrained(string identifier, string? revision = null, string? authToken = null)
    {
        var interop = NativeInteropProvider.Current;
        ArgumentNullException.ThrowIfNull(interop);

        var handle = NativeTokenizerHandle.CreateFromPretrained(identifier, revision, authToken);
        try
        {
            return new Tokenizer(handle, interop);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a tokenizer from a tokenizer.json file stored on disk.
    /// </summary>
    /// <param name="path">Absolute or relative path to the tokenizer configuration file.</param>
    /// <returns>A <see cref="Tokenizer"/> instance initialized from the provided configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or whitespace.</exception>
    /// <exception cref="IOException">Propagates file system exceptions that occur when reading the file.</exception>
    [SuppressMessage("Usage", "MA0032:Use an overload with a CancellationToken", Justification = "File APIs expose synchronous overloads only.")]
    public static Tokenizer FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Tokenizer file path must be provided.", nameof(path));
        }

        var json = File.ReadAllText(path);
        return new Tokenizer(json);
    }

    /// <summary>
    /// Creates a tokenizer from an in-memory tokenizer.json payload.
    /// </summary>
    /// <param name="buffer">UTF-8 encoded tokenizer.json data.</param>
    /// <returns>A <see cref="Tokenizer"/> instance initialized from the buffer.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="buffer"/> is empty.</exception>
    public static Tokenizer FromBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            throw new ArgumentException("Tokenizer buffer cannot be empty.", nameof(buffer));
        }

        var json = Encoding.UTF8.GetString(buffer);
        return new Tokenizer(json);
    }

    private static NativeTokenizerHandle CreateHandleFromJson(string jsonConfig, out INativeInterop interop)
    {
        interop = NativeInteropProvider.Current;
        ArgumentNullException.ThrowIfNull(interop);
        return CreateHandleFromJsonCore(jsonConfig);
    }

    private static NativeTokenizerHandle CreateHandleFromJsonCore(string jsonConfig)
    {
        if (string.IsNullOrWhiteSpace(jsonConfig))
        {
            throw new ArgumentException("Tokenizer configuration JSON must be provided.", nameof(jsonConfig));
        }

        return NativeTokenizerHandle.Create(jsonConfig);
    }

    internal static string NormalizeGenerationConfig(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Generation configuration JSON must be provided.", nameof(json));
        }

        var interop = NativeInteropProvider.Current;
        var nativeResult = interop.TokenizersNormalizeGenerationConfig(json, out var status);
        try
        {
            if (nativeResult == IntPtr.Zero || status != 0)
            {
                var details = interop.GetLastErrorMessage();
                var message = details is null
                    ? "Generation configuration normalization failed."
                    : $"Generation configuration normalization failed: {details}";
                throw new InvalidOperationException(message);
            }

            return Marshal.PtrToStringUTF8(nativeResult) ?? string.Empty;
        }
        finally
        {
            if (nativeResult != IntPtr.Zero)
            {
                interop.FreeString(nativeResult);
            }
        }
    }

    internal static string PlanLogitsProcessors(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Generation configuration JSON must be provided.", nameof(json));
        }

        var interop = NativeInteropProvider.Current;
        var nativeResult = interop.TokenizersPlanLogitsProcessors(json, out var status);
        try
        {
            if (nativeResult == IntPtr.Zero || status != 0)
            {
                var details = interop.GetLastErrorMessage();
                var message = details is null
                    ? "Logits processor planning failed."
                    : $"Logits processor planning failed: {details}";
                throw new InvalidOperationException(message);
            }

            return Marshal.PtrToStringUTF8(nativeResult) ?? string.Empty;
        }
        finally
        {
            if (nativeResult != IntPtr.Zero)
            {
                interop.FreeString(nativeResult);
            }
        }
    }

    internal static string PlanStoppingCriteria(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Generation configuration JSON must be provided.", nameof(json));
        }

        var interop = NativeInteropProvider.Current;
        var nativeResult = interop.TokenizersPlanStoppingCriteria(json, out var status);
        try
        {
            if (nativeResult == IntPtr.Zero || status != 0)
            {
                var details = interop.GetLastErrorMessage();
                var message = details is null
                    ? "Stopping criteria planning failed."
                    : $"Stopping criteria planning failed: {details}";
                throw new InvalidOperationException(message);
            }

            return Marshal.PtrToStringUTF8(nativeResult) ?? string.Empty;
        }
        finally
        {
            if (nativeResult != IntPtr.Zero)
            {
                interop.FreeString(nativeResult);
            }
        }
    }

    /// <summary>
    /// Saves the tokenizer configuration to a JSON file.
    /// </summary>
    /// <param name="path">The file path where the tokenizer configuration will be saved.</param>
    /// <param name="pretty">If <c>true</c>, the JSON output is formatted for readability; otherwise, it is compact.</param>
    /// <remarks>
    /// The output JSON contains the complete tokenizer state, including model, vocabulary, and configuration.
    /// The directory is created if it does not exist.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <c>null</c> or empty.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// tokenizer.Save("tokenizer_backup.json", pretty: true);
    /// </code>
    /// </example>
    public void Save(string path, bool pretty = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Output path must be provided.", nameof(path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = ToJson(pretty);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Enables padding for all subsequent encoding operations.
    /// </summary>
    /// <param name="options">The padding configuration specifying direction (right/left), pad token, and target length.</param>
    /// <remarks>
    /// Padding ensures all encoded sequences have the same length by adding pad tokens.
    /// Right padding (default) is typical for encoder models; left padding is common for decoder-only models.
    /// Padding is applied after truncation if both are enabled.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native operation fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var paddingOptions = new PaddingOptions
    /// {
    ///     Direction = PaddingDirection.Right,
    ///     Length = 512,
    ///     PadToken = "[PAD]",
    /// };
    /// tokenizer.EnablePadding(paddingOptions);
    /// </code>
    /// </example>
    public void EnablePadding(PaddingOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var length = options.Length ?? -1;
                var multiple = options.PadToMultipleOf ?? 0;
                var request = new NativePaddingRequest(
                    (int)options.Direction,
                    options.PadId,
                    options.PadTypeId,
                    options.PadToken,
                    length,
                    multiple);
                var result = _interop.TokenizerEnablePadding(handlePtr, request, out var status);

                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer enable padding failed.");
                }

                return 0;
            });
        }
    }

    /// <summary>
    /// Disables padding for all subsequent encoding operations.
    /// </summary>
    /// <remarks>
    /// After calling this method, encoded sequences retain their natural lengths without padding.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the native operation fails.</exception>
    public void DisablePadding()
    {
        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = _interop.TokenizerDisablePadding(handlePtr, out var status);
                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer disable padding failed.");
                }

                return 0;
            });
        }
    }

    /// <summary>
    /// Retrieves the current padding configuration.
    /// </summary>
    /// <returns>The current <see cref="PaddingOptions"/> if padding is enabled; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if padding is disabled or not configured.
    /// </remarks>
    public PaddingOptions? GetPadding()
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = _interop.TokenizerGetPadding(handlePtr, out var status);
                if (status != 0)
                {
                    if (nativePtr != IntPtr.Zero)
                    {
                        _interop.FreeString(nativePtr);
                    }

                    throw CreateNativeException("Tokenizer padding retrieval failed.");
                }

                if (nativePtr == IntPtr.Zero)
                {
                    return null;
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return null;
                    }

                    return DeserializePaddingOptions(json);
                }
                finally
                {
                    _interop.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Enables truncation for all subsequent encoding operations.
    /// </summary>
    /// <param name="options">The truncation configuration specifying max length, direction, and strategy.</param>
    /// <remarks>
    /// Truncation ensures encoded sequences do not exceed the specified maximum length.
    /// Excess tokens are removed according to the configured strategy (e.g., "longest_first", "only_first").
    /// Truncation is applied before padding if both are enabled.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native operation fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var truncationOptions = new TruncationOptions
    /// {
    ///     MaxLength = 512,
    ///     Strategy = TruncationStrategy.LongestFirst,
    ///     Direction = TruncationDirection.Right,
    /// };
    /// tokenizer.EnableTruncation(truncationOptions);
    /// </code>
    /// </example>
    public void EnableTruncation(TruncationOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = _interop.TokenizerEnableTruncation(
                    handlePtr,
                    (nuint)options.MaxLength,
                    (nuint)options.Stride,
                    (int)options.Strategy,
                    (int)options.Direction,
                    out var status);

                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer enable truncation failed.");
                }

                return 0;
            });
        }
    }

    /// <summary>
    /// Disables truncation for all subsequent encoding operations.
    /// </summary>
    /// <remarks>
    /// After calling this method, sequences are encoded at their natural length without truncation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the native operation fails.</exception>
    public void DisableTruncation()
    {
        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = _interop.TokenizerDisableTruncation(handlePtr, out var status);
                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer disable truncation failed.");
                }

                return 0;
            });
        }
    }

    /// <summary>
    /// Retrieves the current truncation configuration.
    /// </summary>
    /// <returns>The current <see cref="TruncationOptions"/> if truncation is enabled; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// Returns <c>null</c> if truncation is disabled or not configured.
    /// </remarks>
    public TruncationOptions? GetTruncation()
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = _interop.TokenizerGetTruncation(handlePtr, out var status);
                if (status != 0)
                {
                    if (nativePtr != IntPtr.Zero)
                    {
                        _interop.FreeString(nativePtr);
                    }

                    throw CreateNativeException("Tokenizer truncation retrieval failed.");
                }

                if (nativePtr == IntPtr.Zero)
                {
                    return null;
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return null;
                    }

                    return DeserializeTruncationOptions(json);
                }
                finally
                {
                    _interop.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Sets or replaces the tokenizer model.
    /// </summary>
    /// <param name="model">The new model to use. Must be created via <see cref="TokenizerModel"/> factory methods.</param>
    /// <remarks>
    /// This method replaces the tokenizer's current model with a different one.
    /// The model controls how text is tokenized (BPE, WordPiece, Unigram, etc.).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="model"/> is not a valid <see cref="TokenizerModel"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native operation fails.</exception>
    public void SetModel(IModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (model is not TokenizerModel tokenizerModel)
        {
            throw new ArgumentException("Model must be created via TokenizerModel factory methods.", nameof(model));
        }

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                tokenizerModel.InvokeWithHandle(modelPtr =>
                {
                    var result = _interop.TokenizersTokenizerSetModel(handlePtr, modelPtr, out var status);
                    if (result == 0 || status != 0)
                    {
                        throw CreateNativeException("Tokenizer set model failed.");
                    }
                });
            });
        }
    }

    public void SetDecoder(IDecoder decoder)
    {
        if (decoder is null)
        {
            throw new ArgumentNullException(nameof(decoder));
        }

        if (decoder is not TokenizerDecoder tokenizerDecoder)
        {
            throw new ArgumentException("Decoder must be created via TokenizerDecoder factory methods.", nameof(decoder));
        }

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                tokenizerDecoder.InvokeWithHandle(decoderPtr =>
                {
                    var result = _interop.TokenizersTokenizerSetDecoder(handlePtr, decoderPtr, out var status);
                    if (result == 0 || status != 0)
                    {
                        throw CreateNativeException("Tokenizer set decoder failed.");
                    }
                });
            });
        }
    }

    public void ClearDecoder()
    {
        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = _interop.TokenizersTokenizerClearDecoder(handlePtr, out var status);
                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer clear decoder failed.");
                }
            });
        }
    }

    /// <summary>
    /// Encodes a single text string into token IDs.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="addSpecialTokens">If <c>true</c>, special tokens (e.g., [CLS], [SEP]) are prepended/appended as defined by the tokenizer.</param>
    /// <returns>An <see cref="EncodingResult"/> containing token IDs, strings, offsets, and metadata.</returns>
    /// <remarks>
    /// Special tokens are added according to the tokenizer model's configuration.
    /// Padding and truncation are applied based on the settings configured via <see cref="EnablePadding"/> and <see cref="EnableTruncation"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native tokenizer operation fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var result = tokenizer.Encode("Hello, world!");
    /// Console.WriteLine($"Token count: {result.Ids.Count}");
    /// </code>
    /// </example>
    public EncodingResult Encode(string text, bool addSpecialTokens = true)
        => Encode(text, null, addSpecialTokens);

    /// <summary>
    /// Encodes a text pair (e.g., question-answer, premise-hypothesis) into token IDs.
    /// </summary>
    /// <param name="text">The primary text to encode.</param>
    /// <param name="textPair">The secondary text to encode (optional; commonly used for sequence pairs in models like BERT).</param>
    /// <param name="addSpecialTokens">If <c>true</c>, special tokens are inserted between and around the texts as configured.</param>
    /// <returns>An <see cref="EncodingResult"/> containing combined token IDs and metadata for both texts.</returns>
    /// <remarks>
    /// When <paramref name="textPair"/> is provided, the tokenizer inserts separator tokens (e.g., [SEP]) between the two texts.
    /// The resulting token IDs reflect the full sequence with both texts properly demarcated.
    /// Type IDs (segment IDs) in the result distinguish the first text (0) from the second text (1).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native tokenizer operation fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var result = tokenizer.Encode("What is AI?", "Artificial Intelligence is...", addSpecialTokens: true);
    /// // Result contains tokens for the pair with [SEP] between them.
    /// </code>
    /// </example>
    public EncodingResult Encode(string text, string? textPair, bool addSpecialTokens = true)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var encodingPtr = _interop.TokenizerEncode(handlePtr, text, textPair, addSpecialTokens, out var length, out var status);
                if (encodingPtr == IntPtr.Zero || status != 0)
                {
                    throw CreateNativeException("Tokenizer encode failed.");
                }

                try
                {
                    return MarshalEncoding(encodingPtr, length);
                }
                finally
                {
                    _interop.EncodingFree(encodingPtr);
                }
            });
        }
    }

    /// <summary>
    /// Encodes a batch of single text strings in sequence.
    /// </summary>
    /// <param name="inputs">A collection of text strings to encode.</param>
    /// <param name="addSpecialTokens">If <c>true</c>, special tokens are added to each text.</param>
    /// <returns>A read-only list of <see cref="EncodingResult"/> objects, one per input string.</returns>
    /// <remarks>
    /// This method encodes each input string individually and preserves their order.
    /// If padding or truncation is enabled, it is applied to each encoding independently.
    /// This is a convenience method; for high-throughput scenarios, consider direct Encode calls with parallelization.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputs"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection contains <c>null</c> entries.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var texts = new[] { "Hello", "World", "Test" };
    /// var results = tokenizer.EncodeBatch(texts);
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Tokens: {result.Ids.Count}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<EncodingResult> EncodeBatch(IEnumerable<string> inputs, bool addSpecialTokens = true)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var localInputs = inputs.ToArray();
        if (localInputs.Length == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var results = new EncodingResult[localInputs.Length];
        for (var i = 0; i < localInputs.Length; i++)
        {
            if (localInputs[i] is null)
            {
                throw new ArgumentException("Sequence collection cannot contain null entries.", nameof(inputs));
            }

            results[i] = Encode(localInputs[i], null, addSpecialTokens);
        }

        return results;
    }

    /// <summary>
    /// Encodes a batch of text pairs in sequence.
    /// </summary>
    /// <param name="inputs">A collection of text pairs (First, Second) to encode.</param>
    /// <param name="addSpecialTokens">If <c>true</c>, special tokens are inserted per pair.</param>
    /// <returns>A read-only list of <see cref="EncodingResult"/> objects, one per pair, with separator tokens between texts.</returns>
    /// <remarks>
    /// Each pair is encoded with the second text optional (can be <c>null</c>).
    /// The tokenizer inserts special separators between the two texts when present.
    /// Type IDs in the result distinguish the first text from the second.
    /// This is useful for tasks like natural language inference, question-answering, and semantic similarity.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputs"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection contains pairs with <c>null</c> First values.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var pairs = new[]
    /// {
    ///     ("Premise 1", "Hypothesis 1"),
    ///     ("Premise 2", (string?)null),  // Second can be null
    /// };
    /// var results = tokenizer.EncodeBatch(pairs);
    /// </code>
    /// </example>
    public IReadOnlyList<EncodingResult> EncodeBatch(IEnumerable<(string First, string? Second)> inputs, bool addSpecialTokens = true)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var localInputs = inputs.ToArray();
        if (localInputs.Length == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var results = new EncodingResult[localInputs.Length];
        for (var i = 0; i < localInputs.Length; i++)
        {
            if (localInputs[i].First is null)
            {
                throw new ArgumentException("Sequence collection cannot contain null entries.", nameof(inputs));
            }

            results[i] = Encode(localInputs[i].First, localInputs[i].Second, addSpecialTokens);
        }

        return results;
    }

    /// <summary>
    /// Decodes a sequence of token IDs back into text.
    /// </summary>
    /// <param name="ids">A read-only list of token IDs to decode.</param>
    /// <param name="skipSpecialTokens">If <c>true</c>, special tokens (e.g., [CLS], [PAD]) are excluded from the output.</param>
    /// <returns>The decoded text string.</returns>
    /// <remarks>
    /// The decoding process is model-specific and depends on the tokenizer's configuration.
    /// Skipping special tokens produces cleaner output for display purposes.
    /// If an empty sequence is provided, an empty string is returned.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native decode operation fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var ids = new[] { 101, 7592, 1010, 2088, 102 };  // Example token IDs
    /// string text = tokenizer.Decode(ids, skipSpecialTokens: true);
    /// Console.WriteLine(text);  // Output: "Hello, world"
    /// </code>
    /// </example>
    public string Decode(IReadOnlyList<int> ids, bool skipSpecialTokens = true)
    {
        if (ids is null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        if (ids.Count == 0)
        {
            return string.Empty;
        }

        var length = ids.Count;
        var rented = ArrayPool<uint>.Shared.Rent(length);

        try
        {
            for (var i = 0; i < length; i++)
            {
                rented[i] = checked((uint)ids[i]);
            }

            lock (_syncRoot)
            {
                return _handle.InvokeWithHandle(handlePtr =>
                {
                    var nativePtr = _interop.TokenizerDecode(handlePtr, rented, (nuint)length, skipSpecialTokens, out var status);
                    if (nativePtr == IntPtr.Zero || status != 0)
                    {
                        throw CreateNativeException("Tokenizer decode failed.");
                    }

                    try
                    {
                        return Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
                    }
                    finally
                    {
                        _interop.FreeString(nativePtr);
                    }
                });
            }
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(rented, clearArray: true);
        }
    }

    /// <summary>
    /// Decodes a batch of token ID sequences into text strings.
    /// </summary>
    /// <param name="sequences">A collection of token ID sequences to decode.</param>
    /// <param name="skipSpecialTokens">If <c>true</c>, special tokens are excluded from the output.</param>
    /// <returns>A read-only list of decoded text strings, one per sequence, in order.</returns>
    /// <remarks>
    /// Each sequence is decoded independently. Empty sequences result in empty strings.
    /// This is a convenience method for bulk decoding; the order of results matches the input order.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sequences"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection contains <c>null</c> sequences.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the total token count exceeds supported bounds.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// var sequences = new[]
    /// {
    ///     new int[] { 101, 7592, 102 },      // "Hello"
    ///     new int[] { 101, 2088, 102 },      // "World"
    /// };
    /// var texts = tokenizer.DecodeBatch(sequences, skipSpecialTokens: true);
    /// foreach (var text in texts)
    /// {
    ///     Console.WriteLine(text);
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> sequences, bool skipSpecialTokens = true)
    {
        if (sequences is null)
        {
            throw new ArgumentNullException(nameof(sequences));
        }

        var inputs = sequences.ToArray();
        if (inputs.Length == 0)
        {
            return Array.Empty<string>();
        }

        var lengths = new nuint[inputs.Length];
        var totalLength = PopulateSequenceLengths(inputs, lengths, nameof(sequences));

        if (totalLength == 0)
        {
            return CreateEmptyResults(inputs.Length);
        }

        if (totalLength > int.MaxValue)
        {
            throw new InvalidOperationException("Total token count exceeds supported bounds.");
        }

        var flattened = FlattenSequences(inputs, totalLength);
        return DecodeBatchInternal(inputs, lengths, totalLength, flattened, skipSpecialTokens);
    }

    private static nuint PopulateSequenceLengths(IReadOnlyList<int>[] inputs, nuint[] lengths, string parameterName)
    {
        nuint totalLength = 0;
        for (var i = 0; i < inputs.Length; i++)
        {
            var sequence = inputs[i];
            if (sequence is null)
            {
                throw new ArgumentException("Encoding collection cannot contain null entries.", parameterName);
            }

            var length = (nuint)sequence.Count;
            lengths[i] = length;
            totalLength += length;
        }

        return totalLength;
    }

    private static string[] CreateEmptyResults(int count)
    {
        var results = new string[count];
        Array.Fill(results, string.Empty);
        return results;
    }

    private static uint[] FlattenSequences(IReadOnlyList<int>[] inputs, nuint totalLength)
    {
        var flattened = new uint[(int)totalLength];
        var offset = 0;

        for (var i = 0; i < inputs.Length; i++)
        {
            var sequence = inputs[i];
            for (var j = 0; j < sequence.Count; j++)
            {
                flattened[offset++] = checked((uint)sequence[j]);
            }
        }

        return flattened;
    }

    private IReadOnlyList<string> DecodeBatchInternal(
        IReadOnlyList<int>[] inputs,
        nuint[] lengths,
        nuint totalLength,
        uint[] flattened,
        bool skipSpecialTokens)
    {
        var count = inputs.Length;
        var outputPointers = new IntPtr[count];
        var results = new string[count];

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                DecodeBatchNative(handlePtr, flattened, lengths, totalLength, count, skipSpecialTokens, outputPointers);
                PopulateDecodedResults(outputPointers, results);
                return 0;
            });
        }

        return results;
    }

    private void DecodeBatchNative(
        IntPtr handlePtr,
        uint[] flattened,
        nuint[] lengths,
        nuint totalLength,
        int count,
        bool skipSpecialTokens,
        IntPtr[] outputPointers)
    {
        unsafe
        {
            fixed (uint* tokensPtr = flattened)
            fixed (nuint* lengthsPtr = lengths)
            fixed (IntPtr* outputsPtr = outputPointers)
            {
                var request = new NativeDecodeBatchRequest(
                    handlePtr,
                    tokensPtr,
                    totalLength,
                    lengthsPtr,
                    (nuint)count,
                    skipSpecialTokens,
                    outputsPtr);

                var decodedCount = _interop.TokenizerDecodeBatchFlat(request, out var status);
                if (status != 0 || decodedCount != count)
                {
                    ReleaseOutputPointers(outputsPtr, count);
                    throw CreateNativeException("Tokenizer batch decode failed.");
                }
            }
        }
    }

    private void PopulateDecodedResults(IntPtr[] outputPointers, string[] results)
    {
        for (var i = 0; i < outputPointers.Length; i++)
        {
            var nativePtr = outputPointers[i];
            if (nativePtr == IntPtr.Zero)
            {
                results[i] = string.Empty;
                continue;
            }

            try
            {
                results[i] = Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
            }
            finally
            {
                _interop.FreeString(nativePtr);
                outputPointers[i] = IntPtr.Zero;
            }
        }
    }

    private unsafe void ReleaseOutputPointers(IntPtr* outputsPtr, int count)
    {
        for (var index = 0; index < count; index++)
        {
            if (outputsPtr[index] != IntPtr.Zero)
            {
                _interop.FreeString(outputsPtr[index]);
                outputsPtr[index] = IntPtr.Zero;
            }
        }
    }

    internal string ApplyChatTemplate(
        string template,
        string messagesJson,
        string? variablesJson,
        bool addGenerationPrompt)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException("Chat template source must be provided.", nameof(template));
        }

        if (string.IsNullOrWhiteSpace(messagesJson))
        {
            throw new ArgumentException("Chat message payload must be provided.", nameof(messagesJson));
        }

        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = _interop.TokenizerApplyChatTemplate(
                    handlePtr,
                    template,
                    messagesJson,
                    variablesJson,
                    addGenerationPrompt,
                    out var status);

                if (nativePtr == IntPtr.Zero || status != 0)
                {
                    throw CreateNativeException("Tokenizer apply chat template failed.");
                }

                try
                {
                    return Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
                }
                finally
                {
                    _interop.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Retrieves the token ID for a given token string.
    /// </summary>
    /// <param name="token">The token string to look up (e.g., "hello", "[CLS]").</param>
    /// <returns>The token ID if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// This method performs a lookup in the tokenizer's vocabulary.
    /// Returns <c>null</c> for unknown tokens or if the input is <c>null</c> or empty.
    /// Useful for identifying special token IDs or verifying token validity.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// int? id = tokenizer.TokenToId("hello");
    /// if (id.HasValue)
    /// {
    ///     Console.WriteLine($"Token ID: {id}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Token not found");
    /// }
    /// </code>
    /// </example>
    public int? TokenToId(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        lock (_syncRoot)
        {
            var result = _handle.InvokeWithHandle(handlePtr =>
            {
                var id = _interop.TokenToId(handlePtr, token, out var nativeStatus);
                return (Id: id, Status: nativeStatus);
            });

            return result.Status == 0 && result.Id >= 0 ? result.Id : null;
        }
    }

    /// <summary>
    /// Retrieves the token string for a given token ID.
    /// </summary>
    /// <param name="id">The token ID to look up.</param>
    /// <returns>The token string if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// This method performs a reverse lookup in the tokenizer's vocabulary.
    /// Returns <c>null</c> for unknown IDs or if the ID is negative.
    /// Useful for debugging token sequences or understanding decoded output.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// string? token = tokenizer.IdToToken(101);
    /// Console.WriteLine(token ?? "Unknown token");  // e.g., "[CLS]"
    /// </code>
    /// </example>
    public string? IdToToken(int id)
        => id < 0 ? null : IdToToken((uint)id);

    /// <summary>
    /// Retrieves the token string for a given unsigned token ID.
    /// </summary>
    /// <param name="id">The unsigned token ID to look up.</param>
    /// <returns>The token string if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// This overload accepts unsigned IDs and returns <c>null</c> for unknown IDs or lookup failures.
    /// </remarks>
    public string? IdToToken(uint id)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = _interop.IdToToken(handlePtr, id, out var status);
                if (nativePtr == IntPtr.Zero || status != 0)
                {
                    return null;
                }

                try
                {
                    return Marshal.PtrToStringUTF8(nativePtr);
                }
                finally
                {
                    _interop.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Serializes the tokenizer configuration to a JSON string.
    /// </summary>
    /// <param name="pretty">If <c>true</c>, the JSON output is formatted for readability; otherwise, it is compact.</param>
    /// <returns>The complete tokenizer configuration as a JSON string.</returns>
    /// <remarks>
    /// The returned JSON contains the full tokenizer state including model, vocabulary, and settings.
    /// Use <see cref="Save"/> to write to a file directly.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when serialization fails.</exception>
    /// <example>
    /// <code>
    /// var tokenizer = Tokenizer.FromFile("tokenizer.json");
    /// string json = tokenizer.ToJson(pretty: true);
    /// Console.WriteLine(json);
    /// </code>
    /// </example>
    public string ToJson(bool pretty = false)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr => GetConfigJsonUnsafe(handlePtr, pretty));
        }
    }

    /// <summary>
    /// Releases all resources held by the tokenizer.
    /// </summary>
    /// <remarks>
    /// This method releases the native tokenizer handle and suppresses the finalizer.
    /// Always call this method when the tokenizer is no longer needed to free resources promptly.
    /// </remarks>
    public void Dispose()
    {
        _handle.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Retrieves the tokenizer configuration JSON from the native layer.
    /// </summary>
    /// <param name="handlePtr">Native tokenizer pointer.</param>
    /// <param name="pretty">True to request pretty-printed JSON.</param>
    /// <returns>The tokenizer configuration JSON.</returns>
    private string GetConfigJsonUnsafe(IntPtr handlePtr, bool pretty)
    {
        var nativePtr = _interop.TokenizerGetConfig(handlePtr, pretty, out var status);
        if (nativePtr == IntPtr.Zero || status != 0)
        {
            throw CreateNativeException("Tokenizer serialization failed.");
        }

        try
        {
            return Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
        }
        finally
        {
            _interop.FreeString(nativePtr);
        }
    }

    private EncodingResult MarshalEncoding(IntPtr encodingPtr, nuint length)
    {
        if (length == 0)
        {
            return new EncodingResult(Array.Empty<int>(), Array.Empty<string>(), Array.Empty<(int, int)>());
        }

        if (length > int.MaxValue)
        {
            throw new InvalidOperationException("Encoding length exceeds supported bounds.");
        }

        var size = (int)length;
        var managedIds = new int[size];
        var typeIds = new uint[size];
        var attentionMask = new uint[size];
        var specialTokensMask = new uint[size];
        var offsetsNative = new NativeMethods.EncodingOffsetNative[size];
        var wordIdsRaw = new int[size];
        var sequenceIdsRaw = new int[size];
        var numericBuffer = new EncodingNumericBuffer(managedIds, typeIds, attentionMask, specialTokensMask, offsetsNative, wordIdsRaw, sequenceIdsRaw);
        PopulateNumericData(encodingPtr, size, numericBuffer);

        var offsets = BuildOffsets(offsetsNative);
        var wordIds = BuildNullableValues(wordIdsRaw);
        var sequenceIds = BuildNullableValues(sequenceIdsRaw);
        var tokens = ExtractTokens(encodingPtr, size);
        var overflowing = MarshalOverflowingEncodings(encodingPtr);

        return new EncodingResult(
            managedIds,
            tokens,
            offsets,
            typeIds,
            attentionMask,
            specialTokensMask,
            wordIds,
            sequenceIds,
            overflowing);
    }

    private readonly struct EncodingNumericBuffer
    {
        public EncodingNumericBuffer(
            int[] managedIds,
            uint[] typeIds,
            uint[] attentionMask,
            uint[] specialTokensMask,
            NativeMethods.EncodingOffsetNative[] offsetsNative,
            int[] wordIdsRaw,
            int[] sequenceIdsRaw)
        {
            ManagedIds = managedIds;
            TypeIds = typeIds;
            AttentionMask = attentionMask;
            SpecialTokensMask = specialTokensMask;
            OffsetsNative = offsetsNative;
            WordIdsRaw = wordIdsRaw;
            SequenceIdsRaw = sequenceIdsRaw;
        }

        public int[] ManagedIds { get; }

        public uint[] TypeIds { get; }

        public uint[] AttentionMask { get; }

        public uint[] SpecialTokensMask { get; }

        public NativeMethods.EncodingOffsetNative[] OffsetsNative { get; }

        public int[] WordIdsRaw { get; }

        public int[] SequenceIdsRaw { get; }
    }

    private void PopulateNumericData(IntPtr encodingPtr, int size, in EncodingNumericBuffer buffer)
    {
        unsafe
        {
            fixed (int* idsPtr = buffer.ManagedIds)
            fixed (uint* typeIdsPtr = buffer.TypeIds)
            fixed (uint* attentionMaskPtr = buffer.AttentionMask)
            fixed (uint* specialTokensMaskPtr = buffer.SpecialTokensMask)
            fixed (NativeMethods.EncodingOffsetNative* offsetsPtr = buffer.OffsetsNative)
            fixed (int* wordIdsPtr = buffer.WordIdsRaw)
            fixed (int* sequenceIdsPtr = buffer.SequenceIdsRaw)
            {
                var destinations = new NativeMethods.EncodingNumericDest
                {
                    Ids = (IntPtr)idsPtr,
                    TypeIds = (IntPtr)typeIdsPtr,
                    AttentionMask = (IntPtr)attentionMaskPtr,
                    SpecialTokensMask = (IntPtr)specialTokensMaskPtr,
                    Offsets = (IntPtr)offsetsPtr,
                    WordIds = (IntPtr)wordIdsPtr,
                    SequenceIds = (IntPtr)sequenceIdsPtr
                };

                var copied = _interop.EncodingCopyNumeric(encodingPtr, ref destinations, (nuint)size, out var status);
                if (status != 0)
                {
                    throw CreateNativeException("Tokenizer encoding numeric copy failed.");
                }

                if (copied != size)
                {
                    throw new InvalidOperationException("Tokenizer encoding numeric copy returned an unexpected token count.");
                }
            }
        }
    }

    private static (int Start, int End)[] BuildOffsets(NativeMethods.EncodingOffsetNative[] offsetsNative)
    {
        var offsets = new (int Start, int End)[offsetsNative.Length];
        for (var i = 0; i < offsetsNative.Length; i++)
        {
            offsets[i] = ((int)offsetsNative[i].Start, (int)offsetsNative[i].End);
        }

        return offsets;
    }

    private static int?[] BuildNullableValues(int[] source)
    {
        var result = new int?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            result[i] = source[i] >= 0 ? source[i] : null;
        }

        return result;
    }

    private string[] ExtractTokens(IntPtr encodingPtr, int size)
    {
        var tokens = new string[size];
        var tokenPtrBuffer = ArrayPool<IntPtr>.Shared.Rent(size);
        try
        {
            _interop.EncodingGetTokens(encodingPtr, tokenPtrBuffer, (nuint)size);
            for (var i = 0; i < size; i++)
            {
                var tokenPtr = tokenPtrBuffer[i];
                tokens[i] = tokenPtr == IntPtr.Zero
                    ? string.Empty
                    : Marshal.PtrToStringUTF8(tokenPtr) ?? string.Empty;

                if (tokenPtr != IntPtr.Zero)
                {
                    _interop.FreeString(tokenPtr);
                    tokenPtrBuffer[i] = IntPtr.Zero;
                }
            }
        }
        finally
        {
            for (var i = 0; i < size; i++)
            {
                if (tokenPtrBuffer[i] != IntPtr.Zero)
                {
                    _interop.FreeString(tokenPtrBuffer[i]);
                    tokenPtrBuffer[i] = IntPtr.Zero;
                }
            }

            ArrayPool<IntPtr>.Shared.Return(tokenPtrBuffer, clearArray: true);
        }

        return tokens;
    }

    private IReadOnlyList<EncodingResult> MarshalOverflowingEncodings(IntPtr encodingPtr)
    {
        var overflowingCount = (int)_interop.EncodingGetOverflowingCount(encodingPtr);
        if (overflowingCount == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var overflowResults = new List<EncodingResult>(overflowingCount);
        for (var i = 0; i < overflowingCount; i++)
        {
            var overflowingPtr = _interop.EncodingGetOverflowing(encodingPtr, (nuint)i, out var overflowingLength, out var overflowingStatus);
            if (overflowingStatus != 0 || overflowingPtr == IntPtr.Zero)
            {
                continue;
            }

            try
            {
                overflowResults.Add(MarshalEncoding(overflowingPtr, overflowingLength));
            }
            finally
            {
                _interop.EncodingFree(overflowingPtr);
            }
        }

        return overflowResults.Count == 0 ? Array.Empty<EncodingResult>() : overflowResults;
    }

    private InvalidOperationException CreateNativeException(string message)
    {
        var details = _interop.GetLastErrorMessage();
        return details is null
            ? new InvalidOperationException(message)
            : new InvalidOperationException($"{message}: {details}");
    }

    private static PaddingOptions? DeserializePaddingOptions(string json)
    {
        var payload = JsonSerializer.Deserialize<NativePaddingPayload>(json, JsonOptions);
        if (payload is null)
        {
            return null;
        }

        var padToken = string.IsNullOrEmpty(payload.PadToken) ? "[PAD]" : payload.PadToken;
        int? length = null;

        if (payload.Strategy.ValueKind == JsonValueKind.Object &&
            payload.Strategy.TryGetProperty("Fixed", out var fixedLength) &&
            fixedLength.ValueKind == JsonValueKind.Number)
        {
            length = fixedLength.GetInt32();
        }

        var direction = ParsePaddingDirection(payload.Direction);

        try
        {
            return new PaddingOptions(direction, payload.PadId, payload.PadTypeId, padToken, length, payload.PadToMultipleOf);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static TruncationOptions? DeserializeTruncationOptions(string json)
    {
        var payload = JsonSerializer.Deserialize<NativeTruncationPayload>(json, JsonOptions);
        if (payload is null)
        {
            return null;
        }

        var strategy = ParseTruncationStrategy(payload.Strategy);
        var direction = ParseTruncationDirection(payload.Direction);

        try
        {
            return new TruncationOptions(payload.MaxLength, payload.Stride, strategy, direction);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static PaddingDirection ParsePaddingDirection(string? value)
        => string.Equals(value, "left", StringComparison.OrdinalIgnoreCase)
            ? PaddingDirection.Left
            : PaddingDirection.Right;

    private static TruncationDirection ParseTruncationDirection(string? value)
        => string.Equals(value, "left", StringComparison.OrdinalIgnoreCase)
            ? TruncationDirection.Left
            : TruncationDirection.Right;

    private static TruncationStrategy ParseTruncationStrategy(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TruncationStrategy.LongestFirst;
        }

        return value switch
        {
            "longest_first" => TruncationStrategy.LongestFirst,
            "only_first" => TruncationStrategy.OnlyFirst,
            "only_second" => TruncationStrategy.OnlySecond,
            _ when Enum.TryParse(value, true, out TruncationStrategy parsed) => parsed,
            _ => TruncationStrategy.LongestFirst
        };
    }

    private sealed class NativePaddingPayload
    {
        [JsonPropertyName("strategy")]
        public JsonElement Strategy { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "right";

        [JsonPropertyName("pad_to_multiple_of")]
        public int? PadToMultipleOf { get; set; }

        [JsonPropertyName("pad_id")]
        public int PadId { get; set; }

        [JsonPropertyName("pad_type_id")]
        public int PadTypeId { get; set; }

        [JsonPropertyName("pad_token")]
        public string PadToken { get; set; } = "[PAD]";
    }

    private sealed class NativeTruncationPayload
    {
        [JsonPropertyName("direction")]
        public string? Direction { get; set; }

        [JsonPropertyName("max_length")]
        public int MaxLength { get; set; }

        [JsonPropertyName("strategy")]
        public string Strategy { get; set; } = "LongestFirst";

        [JsonPropertyName("stride")]
        public int Stride { get; set; }
    }
}
