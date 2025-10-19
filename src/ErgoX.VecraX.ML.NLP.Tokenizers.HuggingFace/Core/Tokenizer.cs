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

public sealed class Tokenizer : ITokenizer
{
    private readonly NativeTokenizerHandle _handle;
    private readonly object _syncRoot = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private Tokenizer(NativeTokenizerHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        _handle = handle;
    }

    public Tokenizer(string jsonConfig)
        : this(CreateHandleFromJson(jsonConfig))
    {
    }

    public static Tokenizer FromPretrained(string identifier, string? revision = null, string? authToken = null)
    {
        var handle = NativeTokenizerHandle.CreateFromPretrained(identifier, revision, authToken);
        return new Tokenizer(handle);
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

    private static NativeTokenizerHandle CreateHandleFromJson(string jsonConfig)
    {
        if (string.IsNullOrWhiteSpace(jsonConfig))
        {
            throw new ArgumentException("Tokenizer configuration JSON must be provided.", nameof(jsonConfig));
        }

        return NativeTokenizerHandle.Create(jsonConfig);
    }

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
                var result = NativeMethods.TokenizerEnablePadding(
                    handlePtr,
                    (int)options.Direction,
                    options.PadId,
                    options.PadTypeId,
                    options.PadToken,
                    length,
                    multiple,
                    out var status);

                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer enable padding failed.");
                }

                return 0;
            });
        }
    }

    public void DisablePadding()
    {
        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = NativeMethods.TokenizerDisablePadding(handlePtr, out var status);
                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer disable padding failed.");
                }

                return 0;
            });
        }
    }

    public PaddingOptions? GetPadding()
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = NativeMethods.TokenizerGetPadding(handlePtr, out var status);
                if (status != 0)
                {
                    if (nativePtr != IntPtr.Zero)
                    {
                        NativeMethods.FreeString(nativePtr);
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
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

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
                var result = NativeMethods.TokenizerEnableTruncation(
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

    public void DisableTruncation()
    {
        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                var result = NativeMethods.TokenizerDisableTruncation(handlePtr, out var status);
                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer disable truncation failed.");
                }

                return 0;
            });
        }
    }

    public TruncationOptions? GetTruncation()
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = NativeMethods.TokenizerGetTruncation(handlePtr, out var status);
                if (status != 0)
                {
                    if (nativePtr != IntPtr.Zero)
                    {
                        NativeMethods.FreeString(nativePtr);
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
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

    public EncodingResult Encode(string sequence, bool addSpecialTokens = true)
        => Encode(sequence, null, addSpecialTokens);

    public EncodingResult Encode(string sequence, string? pair, bool addSpecialTokens = true)
    {
        if (sequence is null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var encodingPtr = NativeMethods.TokenizerEncode(handlePtr, sequence, pair, addSpecialTokens, out var length, out var status);
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
                    NativeMethods.EncodingFree(encodingPtr);
                }
            });
        }
    }

    public IReadOnlyList<EncodingResult> EncodeBatch(IEnumerable<string> sequences, bool addSpecialTokens = true)
    {
        if (sequences is null)
        {
            throw new ArgumentNullException(nameof(sequences));
        }

        var inputs = sequences.ToArray();
        if (inputs.Length == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var results = new EncodingResult[inputs.Length];
        for (var i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] is null)
            {
                throw new ArgumentException("Sequence collection cannot contain null entries.", nameof(sequences));
            }

            results[i] = Encode(inputs[i], null, addSpecialTokens);
        }

        return results;
    }

    public IReadOnlyList<EncodingResult> EncodeBatch(IEnumerable<(string First, string? Second)> sequences, bool addSpecialTokens = true)
    {
        if (sequences is null)
        {
            throw new ArgumentNullException(nameof(sequences));
        }

        var inputs = sequences.ToArray();
        if (inputs.Length == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var results = new EncodingResult[inputs.Length];
        for (var i = 0; i < inputs.Length; i++)
        {
            if (inputs[i].First is null)
            {
                throw new ArgumentException("Sequence collection cannot contain null entries.", nameof(sequences));
            }

            results[i] = Encode(inputs[i].First, inputs[i].Second, addSpecialTokens);
        }

        return results;
    }

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
                    var nativePtr = NativeMethods.TokenizerDecode(handlePtr, rented, (nuint)length, skipSpecialTokens, out var status);
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
                        NativeMethods.FreeString(nativePtr);
                    }
                });
            }
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(rented, clearArray: true);
        }
    }

    public IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> encodings, bool skipSpecialTokens = true)
    {
        if (encodings is null)
        {
            throw new ArgumentNullException(nameof(encodings));
        }

        var inputs = encodings.ToArray();
        if (inputs.Length == 0)
        {
            return Array.Empty<string>();
        }

        var count = inputs.Length;
        var lengths = new nuint[count];
        nuint totalLength = 0;

        for (var i = 0; i < count; i++)
        {
            var sequence = inputs[i];
            if (sequence is null)
            {
                throw new ArgumentException("Encoding collection cannot contain null entries.", nameof(encodings));
            }

            lengths[i] = (nuint)sequence.Count;
            totalLength += lengths[i];
        }

        if (totalLength == 0)
        {
            return Enumerable.Repeat(string.Empty, count).ToArray();
        }

        if (totalLength > int.MaxValue)
        {
            throw new InvalidOperationException("Total token count exceeds supported bounds.");
        }

        var flattened = new uint[(int)totalLength];
        var offset = 0;
        for (var i = 0; i < count; i++)
        {
            var sequence = inputs[i];
            for (var j = 0; j < sequence.Count; j++)
            {
                flattened[offset++] = checked((uint)sequence[j]);
            }
        }

        var outputPointers = new IntPtr[count];
        var results = new string[count];

        lock (_syncRoot)
        {
            _handle.InvokeWithHandle(handlePtr =>
            {
                unsafe
                {
                    fixed (uint* tokensPtr = flattened)
                    fixed (nuint* lengthsPtr = lengths)
                    fixed (IntPtr* outputsPtr = outputPointers)
                    {
                        var decodedCount = NativeMethods.TokenizerDecodeBatchFlat(
                            handlePtr,
                            tokensPtr,
                            (nuint)totalLength,
                            lengthsPtr,
                            (nuint)count,
                            skipSpecialTokens,
                            outputsPtr,
                            out var status);

                        if (status != 0 || decodedCount != count)
                        {
                            for (var index = 0; index < count; index++)
                            {
                                if (outputsPtr[index] != IntPtr.Zero)
                                {
                                    NativeMethods.FreeString(outputsPtr[index]);
                                    outputsPtr[index] = IntPtr.Zero;
                                }
                            }

                            throw CreateNativeException("Tokenizer batch decode failed.");
                        }
                    }
                }

                for (var i = 0; i < count; i++)
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
                        NativeMethods.FreeString(nativePtr);
                        outputPointers[i] = IntPtr.Zero;
                    }
                }

                return 0;
            });
        }

        return results;
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
                var nativePtr = NativeMethods.TokenizerApplyChatTemplate(
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
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

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
                var id = NativeMethods.TokenToId(handlePtr, token, out var nativeStatus);
                return (Id: id, Status: nativeStatus);
            });

            return result.Status == 0 && result.Id >= 0 ? result.Id : null;
        }
    }

    public string? IdToToken(int id)
        => id < 0 ? null : IdToToken((uint)id);

    public string? IdToToken(uint id)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = NativeMethods.IdToToken(handlePtr, id, out var status);
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
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

    public string ToJson(bool pretty = false)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr => GetConfigJsonUnsafe(handlePtr, pretty));
        }
    }

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
    private static string GetConfigJsonUnsafe(IntPtr handlePtr, bool pretty)
    {
        var nativePtr = NativeMethods.TokenizerGetConfig(handlePtr, pretty, out var status);
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
            NativeMethods.FreeString(nativePtr);
        }
    }

    private static EncodingResult MarshalEncoding(IntPtr encodingPtr, nuint length)
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
        var offsets = new (int Start, int End)[size];
        var wordIdsRaw = new int[size];
        var sequenceIdsRaw = new int[size];
        var wordIds = new int?[size];
        var sequenceIds = new int?[size];

        unsafe
        {
            fixed (int* idsPtr = managedIds)
            fixed (uint* typeIdsPtr = typeIds)
            fixed (uint* attentionMaskPtr = attentionMask)
            fixed (uint* specialTokensMaskPtr = specialTokensMask)
            fixed (NativeMethods.EncodingOffsetNative* offsetsPtr = offsetsNative)
            fixed (int* wordIdsPtr = wordIdsRaw)
            fixed (int* sequenceIdsPtr = sequenceIdsRaw)
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

                var copied = NativeMethods.EncodingCopyNumeric(encodingPtr, ref destinations, (nuint)size, out var status);
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

        for (var i = 0; i < size; i++)
        {
            offsets[i] = ((int)offsetsNative[i].Start, (int)offsetsNative[i].End);
            wordIds[i] = wordIdsRaw[i] >= 0 ? wordIdsRaw[i] : null;
            sequenceIds[i] = sequenceIdsRaw[i] >= 0 ? sequenceIdsRaw[i] : null;
        }

        var tokens = new string[size];
        var tokenPtrBuffer = ArrayPool<IntPtr>.Shared.Rent(size);
        try
        {
            NativeMethods.EncodingGetTokens(encodingPtr, tokenPtrBuffer, (nuint)size);
            for (var i = 0; i < size; i++)
            {
                var tokenPtr = tokenPtrBuffer[i];
                tokens[i] = tokenPtr == IntPtr.Zero
                    ? string.Empty
                    : Marshal.PtrToStringUTF8(tokenPtr) ?? string.Empty;

                if (tokenPtr != IntPtr.Zero)
                {
                    NativeMethods.FreeString(tokenPtr);
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
                    NativeMethods.FreeString(tokenPtrBuffer[i]);
                    tokenPtrBuffer[i] = IntPtr.Zero;
                }
            }

            ArrayPool<IntPtr>.Shared.Return(tokenPtrBuffer, clearArray: true);
        }

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

    private static IReadOnlyList<EncodingResult> MarshalOverflowingEncodings(IntPtr encodingPtr)
    {
        var overflowingCount = (int)NativeMethods.EncodingGetOverflowingCount(encodingPtr);
        if (overflowingCount == 0)
        {
            return Array.Empty<EncodingResult>();
        }

        var overflowResults = new List<EncodingResult>(overflowingCount);
        for (var i = 0; i < overflowingCount; i++)
        {
            var overflowingPtr = NativeMethods.EncodingGetOverflowing(encodingPtr, (nuint)i, out var overflowingLength, out var overflowingStatus);
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
                NativeMethods.EncodingFree(overflowingPtr);
            }
        }

        return overflowResults.Count == 0 ? Array.Empty<EncodingResult>() : overflowResults;
    }

    private static InvalidOperationException CreateNativeException(string message)
    {
        var details = NativeMethods.GetLastErrorMessage();
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

    private static string SerializePaddingDirection(PaddingDirection direction)
        => direction == PaddingDirection.Left ? "left" : "right";

    private static string SerializeTruncationDirection(TruncationDirection direction)
        => direction == TruncationDirection.Left ? "left" : "right";

    private static string SerializeTruncationStrategy(TruncationStrategy strategy)
        => strategy switch
        {
            TruncationStrategy.OnlyFirst => "only_first",
            TruncationStrategy.OnlySecond => "only_second",
            _ => "longest_first"
        };

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
