using System;
using System.Collections.Generic;
using System.Buffers;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Options;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal;
using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace;

public sealed class Tokenizer : IDisposable
{
    private readonly NativeTokenizerHandle _handle;
    private readonly object _syncRoot = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private static readonly JsonSerializerOptions AddedTokenSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions AddedTokenDecoderJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Uri HuggingFaceBaseUri = new("https://huggingface.co/");

    private static readonly Lazy<HttpClient> SharedHttpClient = new(CreateSharedHttpClient);

    public Tokenizer(string jsonConfig)
    {
        if (string.IsNullOrWhiteSpace(jsonConfig))
        {
            throw new ArgumentException("Tokenizer configuration JSON must be provided.", nameof(jsonConfig));
        }

        _handle = NativeTokenizerHandle.Create(jsonConfig);
        Config = TokenizerConfig.FromJson(jsonConfig);
    }

    public TokenizerConfig Config { get; private set; }

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

    /// <summary>
    /// Downloads a pretrained tokenizer configuration from Hugging Face synchronously.
    /// </summary>
    /// <param name="identifier">The Hugging Face model identifier (e.g. "distilbert/distilbert-base-uncased").</param>
    /// <param name="options">Optional download customizations such as revision, auth token, or custom client.</param>
    /// <returns>A <see cref="Tokenizer"/> initialized from the downloaded configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the download request returns a non-success status.</exception>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Tokenizer ownership is transferred to the caller.")]
    public static Tokenizer FromPretrained(string identifier, PretrainedTokenizerOptions? options = null)
        => FromPretrainedAsync(identifier, options, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Downloads a pretrained tokenizer configuration from Hugging Face asynchronously.
    /// </summary>
    /// <param name="identifier">The Hugging Face model identifier (e.g. "distilbert/distilbert-base-uncased").</param>
    /// <param name="options">Optional download customizations such as revision, auth token, or custom client.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns>A task that produces a <see cref="Tokenizer"/> initialized from the downloaded configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the download request returns a non-success status.</exception>
    public static async Task<Tokenizer> FromPretrainedAsync(
        string identifier,
        PretrainedTokenizerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Model identifier must be provided.", nameof(identifier));
        }

        options ??= new PretrainedTokenizerOptions();
        options.Validate();

        var requestUri = BuildPretrainedUri(identifier, options.Revision);
        var client = options.HttpClient ?? SharedHttpClient.Value;

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(options.AuthToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AuthToken);
        }

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException($"Tokenizer download failed ({(int)response.StatusCode} {response.ReasonPhrase}): {detail}");
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new Tokenizer(payload);
    }

    /// <summary>
    /// Persists the tokenizer configuration to disk synchronously.
    /// </summary>
    /// <param name="path">Destination path for the tokenizer.json payload.</param>
    /// <param name="pretty">True to format the JSON payload with indentation.</param>
    public void Save(string path, bool pretty = false)
        => Save(path, pretty, cancellationToken: CancellationToken.None);

    /// <summary>
    /// Persists the tokenizer configuration to disk while honoring cancellation.
    /// </summary>
    /// <param name="path">Destination path for the tokenizer.json payload.</param>
    /// <param name="pretty">True to format the JSON payload with indentation.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or whitespace.</exception>
    public void Save(string path, bool pretty, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Output path must be provided.", nameof(path));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = ToJson(pretty);
        File.WriteAllTextAsync(path, json, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Retrieves the vocabulary from the underlying tokenizer.
    /// </summary>
    /// <param name="includeAddedTokens">True to combine runtime-added tokens with the base vocabulary.</param>
    /// <returns>A read-only dictionary mapping token strings to token identifiers.</returns>
    public IReadOnlyDictionary<string, int> GetVocab(bool includeAddedTokens = false)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = NativeMethods.TokenizerGetVocab(handlePtr, includeAddedTokens, out var status);
                if (nativePtr == IntPtr.Zero || status != 0)
                {
                    throw CreateNativeException("Tokenizer vocab retrieval failed.");
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr) ?? "{}";
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOptions) ?? new Dictionary<string, int>();
                    var managed = new Dictionary<string, int>(parsed, StringComparer.Ordinal);

                    if (!includeAddedTokens)
                    {
                        Config.Vocab = new Dictionary<string, int>(managed, StringComparer.Ordinal);
                    }

                    return new ReadOnlyDictionary<string, int>(managed);
                }
                finally
                {
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Adds standard tokens to the tokenizer vocabulary.
    /// </summary>
    /// <param name="tokens">Collection of token strings to add.</param>
    /// <returns>The number of tokens successfully registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection contains null entries.</exception>
    public int AddTokens(IEnumerable<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var prepared = new List<AddedToken>();
        foreach (var token in tokens)
        {
            if (token is null)
            {
                throw new ArgumentException("Token collection cannot contain null entries.", nameof(tokens));
            }

            prepared.Add(new AddedToken(token));
        }

        return AddTokensInternal(prepared, treatAsSpecial: false, "Tokenizer add tokens failed.");
    }

    /// <summary>
    /// Adds fully-specified token descriptors to the tokenizer vocabulary.
    /// </summary>
    /// <param name="tokens">Collection of token descriptors to add.</param>
    /// <returns>The number of tokens successfully registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
    public int AddTokens(IEnumerable<AddedToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        return AddTokensInternal(tokens, treatAsSpecial: false, "Tokenizer add tokens failed.");
    }

    /// <summary>
    /// Adds special tokens to the tokenizer vocabulary.
    /// </summary>
    /// <param name="tokens">Collection of token strings that should be treated as special.</param>
    /// <returns>The number of tokens successfully registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection contains null entries.</exception>
    public int AddSpecialTokens(IEnumerable<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var prepared = new List<AddedToken>();
        foreach (var token in tokens)
        {
            if (token is null)
            {
                throw new ArgumentException("Token collection cannot contain null entries.", nameof(tokens));
            }

            prepared.Add(new AddedToken(token, isSpecial: true, normalized: false));
        }

        return AddTokensInternal(prepared, treatAsSpecial: true, "Tokenizer add special tokens failed.");
    }

    /// <summary>
    /// Adds fully-specified special token descriptors to the tokenizer vocabulary.
    /// </summary>
    /// <param name="tokens">Collection of token descriptors that should be treated as special.</param>
    /// <returns>The number of tokens successfully registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
    public int AddSpecialTokens(IEnumerable<AddedToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        return AddTokensInternal(tokens, treatAsSpecial: true, "Tokenizer add special tokens failed.");
    }

    /// <summary>
    /// Retrieves the decoder metadata for runtime-added tokens.
    /// </summary>
    /// <returns>A read-only dictionary keyed by token identifier with <see cref="AddedToken"/> descriptors.</returns>
    public IReadOnlyDictionary<int, AddedToken> GetAddedTokensDecoder()
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var nativePtr = NativeMethods.TokenizerGetAddedTokensDecoder(handlePtr, out var status);
                if (status != 0)
                {
                    if (nativePtr != IntPtr.Zero)
                    {
                        NativeMethods.FreeString(nativePtr);
                    }

                    throw CreateNativeException("Tokenizer added tokens decoder retrieval failed.");
                }

                if (nativePtr == IntPtr.Zero)
                {
                    return new ReadOnlyDictionary<int, AddedToken>(new Dictionary<int, AddedToken>());
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new ReadOnlyDictionary<int, AddedToken>(new Dictionary<int, AddedToken>());
                    }

                    var entries = JsonSerializer.Deserialize<List<AddedTokenDecoderEntry>>(json!, AddedTokenDecoderJsonOptions)
                        ?? new List<AddedTokenDecoderEntry>();

                    var map = new Dictionary<int, AddedToken>(entries.Count);
                    foreach (var entry in entries)
                    {
                        map[entry.Id] = new AddedToken(entry.Content, entry.Special, entry.SingleWord, entry.LeftStrip, entry.RightStrip, entry.Normalized);
                    }

                    return new ReadOnlyDictionary<int, AddedToken>(map);
                }
                finally
                {
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether encode operations include special tokens by default.
    /// </summary>
    public bool EncodeSpecialTokens
    {
        get
        {
            lock (_syncRoot)
            {
                return _handle.InvokeWithHandle(handlePtr =>
                {
                    var result = NativeMethods.TokenizerGetEncodeSpecialTokens(handlePtr, out var status);
                    if (status != 0)
                    {
                        throw CreateNativeException("Tokenizer encode special tokens retrieval failed.");
                    }

                    return result;
                });
            }
        }
        set
        {
            lock (_syncRoot)
            {
                _handle.InvokeWithHandle(handlePtr =>
                {
                    var outcome = NativeMethods.TokenizerSetEncodeSpecialTokens(handlePtr, value, out var status);
                    if (outcome == 0 || status != 0)
                    {
                        throw CreateNativeException("Tokenizer encode special tokens update failed.");
                    }

                    RefreshConfigUnsafe(handlePtr);
                    return 0;
                });
            }
        }
    }

    /// <summary>
    /// Calculates how many special tokens the post-processor would add for a given sequence shape.
    /// </summary>
    /// <param name="isPairSequence">True when evaluating pair sequences, false for single sequences.</param>
    /// <returns>The number of special tokens that would be appended.</returns>
    public int NumSpecialTokensToAdd(bool isPairSequence = false)
    {
        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                var count = NativeMethods.TokenizerNumSpecialTokensToAdd(handlePtr, isPairSequence, out var status);
                if (status != 0)
                {
                    throw CreateNativeException("Tokenizer special token count retrieval failed.");
                }

                return count;
            });
        }
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
                    (uint)options.PadId,
                    (uint)options.PadTypeId,
                    options.PadToken,
                    length,
                    multiple,
                    out var status);

                if (result == 0 || status != 0)
                {
                    throw CreateNativeException("Tokenizer enable padding failed.");
                }

                UpdatePaddingConfig(options);
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

                Config.Padding = null;
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
                    Config.Padding = null;
                    return null;
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Config.Padding = null;
                        return null;
                    }

                    var options = DeserializePaddingOptions(json);
                    if (options is not null)
                    {
                        UpdatePaddingConfig(options);
                    }
                    else
                    {
                        Config.Padding = null;
                    }

                    return options;
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

                UpdateTruncationConfig(options);
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

                Config.Truncation = null;
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
                    Config.Truncation = null;
                    return null;
                }

                try
                {
                    var json = Marshal.PtrToStringUTF8(nativePtr);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Config.Truncation = null;
                        return null;
                    }

                    var options = DeserializeTruncationOptions(json);
                    if (options is not null)
                    {
                        UpdateTruncationConfig(options);
                    }
                    else
                    {
                        Config.Truncation = null;
                    }

                    return options;
                }
                finally
                {
                    NativeMethods.FreeString(nativePtr);
                }
            });
        }
    }

    public EncodingResult Encode(string sequence, string? pair = null, bool addSpecialTokens = true)
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

    [SuppressMessage("Design", "MA0045:Provide an asynchronous alternative", Justification = "Native encoder is synchronous; wrapper emulates async via Task.Run.")]
    public Task<EncodingResult> EncodeAsync(string sequence, string? pair = null, bool addSpecialTokens = true, CancellationToken cancellationToken = default)
        => Task.Run(() => Encode(sequence, pair, addSpecialTokens), cancellationToken);

    [SuppressMessage("Design", "MA0045:Provide an asynchronous alternative", Justification = "Native encoder is synchronous; wrapper emulates async via Task.Run.")]
    public Task<IReadOnlyList<EncodingResult>> EncodeBatchAsync(IEnumerable<string> sequences, bool addSpecialTokens = true, CancellationToken cancellationToken = default)
        => Task.Run(() => EncodeBatch(sequences, addSpecialTokens), cancellationToken);

    [SuppressMessage("Design", "MA0045:Provide an asynchronous alternative", Justification = "Native encoder is synchronous; wrapper emulates async via Task.Run.")]
    public Task<IReadOnlyList<EncodingResult>> EncodeBatchAsync(IEnumerable<(string First, string? Second)> sequences, bool addSpecialTokens = true, CancellationToken cancellationToken = default)
        => Task.Run(() => EncodeBatch(sequences, addSpecialTokens), cancellationToken);

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
        var results = new string[count];

        var sequencePointers = ArrayPool<IntPtr>.Shared.Rent(count);
        var lengthBuffer = ArrayPool<nuint>.Shared.Rent(count);
        var outputPointers = ArrayPool<IntPtr>.Shared.Rent(count);
        var pinnedHandles = new GCHandle[count];
        var rentedBuffers = new uint[count][];

        try
        {
            // Pin each sequence so the native batch decoder can consume a single pointer table.
            for (var i = 0; i < count; i++)
            {
                var sequence = inputs[i];
                if (sequence is null)
                {
                    throw new ArgumentException("Encoding collection cannot contain null entries.", nameof(encodings));
                }

                var length = sequence.Count;
                lengthBuffer[i] = (nuint)length;

                if (length == 0)
                {
                    sequencePointers[i] = IntPtr.Zero;
                    continue;
                }

                var buffer = ArrayPool<uint>.Shared.Rent(length);
                var target = buffer.AsSpan(0, length);
                for (var j = 0; j < length; j++)
                {
                    target[j] = checked((uint)sequence[j]);
                }

                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                pinnedHandles[i] = handle;
                rentedBuffers[i] = buffer;
                sequencePointers[i] = handle.AddrOfPinnedObject();
            }

            Array.Clear(outputPointers, 0, count);

            lock (_syncRoot)
            {
                _handle.InvokeWithHandle(handlePtr =>
                {
                    unsafe
                    {
                        fixed (IntPtr* sequencesPtr = sequencePointers)
                        fixed (nuint* lengthsPtr = lengthBuffer)
                        fixed (IntPtr* outputsPtr = outputPointers)
                        {
                            var decodedCount = NativeMethods.TokenizerDecodeBatch(
                                handlePtr,
                                sequencesPtr,
                                lengthsPtr,
                                (nuint)count,
                                skipSpecialTokens,
                                outputsPtr,
                                out var status);

                            if (status != 0 || decodedCount != count)
                            {
                                for (var index = 0; index < count; index++)
                                {
                                    if (outputPointers[index] != IntPtr.Zero)
                                    {
                                        NativeMethods.FreeString(outputPointers[index]);
                                        outputPointers[index] = IntPtr.Zero;
                                    }
                                }

                                throw CreateNativeException("Tokenizer batch decode failed.");
                            }

                            for (var index = 0; index < count; index++)
                            {
                                var nativePtr = outputPointers[index];
                                if (nativePtr == IntPtr.Zero)
                                {
                                    results[index] = string.Empty;
                                    continue;
                                }

                                try
                                {
                                    results[index] = Marshal.PtrToStringUTF8(nativePtr) ?? string.Empty;
                                }
                                finally
                                {
                                    NativeMethods.FreeString(nativePtr);
                                    outputPointers[index] = IntPtr.Zero;
                                }
                            }
                        }
                    }

                    return 0;
                });
            }

            return results;
        }
        finally
        {
            for (var i = 0; i < count; i++)
            {
                if (pinnedHandles[i].IsAllocated)
                {
                    pinnedHandles[i].Free();
                }

                if (rentedBuffers[i] is not null)
                {
                    ArrayPool<uint>.Shared.Return(rentedBuffers[i], clearArray: true);
                }
            }

            ArrayPool<IntPtr>.Shared.Return(sequencePointers, clearArray: true);
            ArrayPool<nuint>.Shared.Return(lengthBuffer, clearArray: true);
            ArrayPool<IntPtr>.Shared.Return(outputPointers, clearArray: true);
        }
    }

    [SuppressMessage("Design", "MA0045:Provide an asynchronous alternative", Justification = "Native decoder is synchronous; wrapper emulates async via Task.Run.")]
    public Task<string> DecodeAsync(IReadOnlyList<int> ids, bool skipSpecialTokens = true, CancellationToken cancellationToken = default)
        => Task.Run(() => Decode(ids, skipSpecialTokens), cancellationToken);

    [SuppressMessage("Design", "MA0045:Provide an asynchronous alternative", Justification = "Native decoder is synchronous; wrapper emulates async via Task.Run.")]
    public Task<IReadOnlyList<string>> DecodeBatchAsync(IEnumerable<IReadOnlyList<int>> encodings, bool skipSpecialTokens = true, CancellationToken cancellationToken = default)
        => Task.Run(() => DecodeBatch(encodings, skipSpecialTokens), cancellationToken);

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
    /// Serializes token descriptors and invokes the corresponding native add-tokens routine.
    /// </summary>
    /// <param name="tokens">Tokens to serialize and send to the native tokenizer.</param>
    /// <param name="treatAsSpecial">True to route the operation through <c>add_special_tokens</c>.</param>
    /// <param name="errorContext">Contextual message used when surface exceptions.</param>
    /// <returns>The number of tokens registered by the native tokenizer.</returns>
    /// <exception cref="ArgumentException">Thrown when the token list contains null entries.</exception>
    private int AddTokensInternal(IEnumerable<AddedToken> tokens, bool treatAsSpecial, string errorContext)
    {
        var descriptors = new List<AddedTokenDescriptor>();
        foreach (var token in tokens)
        {
            if (token is null)
            {
                throw new ArgumentException("Token collection cannot contain null entries.", nameof(tokens));
            }

            var special = treatAsSpecial || token.IsSpecial;
            descriptors.Add(new AddedTokenDescriptor(
                token.Content,
                token.SingleWord,
                token.LeftStrip,
                token.RightStrip,
                token.Normalized,
                special));
        }

        if (descriptors.Count == 0)
        {
            return 0;
        }

        var payload = JsonSerializer.Serialize(descriptors, AddedTokenSerializerOptions);

        lock (_syncRoot)
        {
            return _handle.InvokeWithHandle(handlePtr =>
            {
                int status;
                var count = treatAsSpecial
                    ? NativeMethods.TokenizerAddSpecialTokens(handlePtr, payload, out status)
                    : NativeMethods.TokenizerAddTokens(handlePtr, payload, out status);

                if (status != 0)
                {
                    throw CreateNativeException(errorContext);
                }

                RefreshConfigUnsafe(handlePtr);
                return count;
            });
        }
    }

    /// <summary>
    /// Pulls the latest tokenizer config JSON from the native handle and updates <see cref="Config"/>.
    /// </summary>
    /// <param name="handlePtr">Native tokenizer pointer.</param>
    private void RefreshConfigUnsafe(IntPtr handlePtr)
    {
        var json = GetConfigJsonUnsafe(handlePtr, pretty: false);
        Config = TokenizerConfig.FromJson(json);
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

    private static HttpClient CreateSharedHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ErgoX.Tokenizers.NET/1.0");
        return client;
    }

    private static Uri BuildPretrainedUri(string identifier, string revision)
    {
        var segments = identifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            throw new ArgumentException("Model identifier must contain at least one non-empty segment.", nameof(identifier));
        }

        var builder = new StringBuilder();
        foreach (var segment in segments)
        {
            if (segment.Length == 0)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('/');
            }

            builder.Append(Uri.EscapeDataString(segment));
        }

        if (builder.Length == 0)
        {
            throw new ArgumentException("Model identifier must contain valid characters.", nameof(identifier));
        }

        var effectiveRevision = string.IsNullOrWhiteSpace(revision) ? "main" : revision;
        var path = $"{builder}/resolve/{Uri.EscapeDataString(effectiveRevision)}/tokenizer.json";
        return new Uri(HuggingFaceBaseUri, path);
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

    private void UpdatePaddingConfig(PaddingOptions options)
    {
        Config.Padding = new TokenizerConfig.SerializedPadding
        {
            Direction = SerializePaddingDirection(options.Direction),
            PadId = options.PadId,
            PadTypeId = options.PadTypeId,
            PadToken = options.PadToken,
            Length = options.Length,
            PadToMultipleOf = options.PadToMultipleOf
        };
    }

    private void UpdateTruncationConfig(TruncationOptions options)
    {
        Config.Truncation = new TokenizerConfig.SerializedTruncation
        {
            Direction = SerializeTruncationDirection(options.Direction),
            MaxLength = options.MaxLength,
            Stride = options.Stride,
            Strategy = SerializeTruncationStrategy(options.Strategy)
        };
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

    private sealed record AddedTokenDescriptor(
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("single_word")] bool SingleWord,
        [property: JsonPropertyName("lstrip")] bool LeftStrip,
        [property: JsonPropertyName("rstrip")] bool RightStrip,
        [property: JsonPropertyName("normalized")] bool Normalized,
        [property: JsonPropertyName("special")] bool Special);

    private sealed record AddedTokenDecoderEntry(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("single_word")] bool SingleWord,
        [property: JsonPropertyName("lstrip")] bool LeftStrip,
        [property: JsonPropertyName("rstrip")] bool RightStrip,
        [property: JsonPropertyName("normalized")] bool Normalized,
        [property: JsonPropertyName("special")] bool Special);

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
