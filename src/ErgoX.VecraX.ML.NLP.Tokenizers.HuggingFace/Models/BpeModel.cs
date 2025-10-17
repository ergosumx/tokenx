using System.Text.Json;

using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;

/// <summary>
/// Byte-Pair Encoding (BPE) model used by GPT-2, GPT-3, RoBERTa, and similar transformers.
/// BPE segments text by iteratively merging the most frequent pairs of bytes/characters.
/// </summary>
/// <remarks>
/// BPE is a subword tokenization algorithm that learns a vocabulary of variable-length tokens
/// by iteratively merging the most frequent character or byte pairs in the training corpus.
/// This allows it to handle out-of-vocabulary words by breaking them into known subword units.
///
/// Common models using BPE:
/// - GPT-2 (OpenAI)
/// - GPT-3/GPT-4 (OpenAI)
/// - RoBERTa (Facebook AI)
/// - BART (Facebook AI)
/// - GPT-J, GPT-NeoX (EleutherAI)
/// </remarks>
public sealed class BpeModel : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new BPE model with the specified vocabulary and merges.
    /// </summary>
    /// <param name="vocab">Dictionary mapping tokens to their IDs</param>
    /// <param name="merges">List of merge rules as (token1, token2) pairs</param>
    /// <param name="cacheCapacity">Optional cache size for tokenization speed optimization (default: 10000)</param>
    /// <param name="dropout">Optional dropout probability for BPE merge operations (0.0-1.0, null = no dropout)</param>
    /// <param name="unknownToken">Token to use for unknown characters (default: null)</param>
    /// <param name="continuingSubwordPrefix">Prefix for non-initial subword tokens (e.g., "##" for BERT-style)</param>
    /// <param name="endOfWordSuffix">Suffix for end-of-word tokens (e.g., "&lt;/w&gt;")</param>
    /// <param name="fuseUnknown">Whether to fuse consecutive unknown tokens into a single token</param>
    /// <param name="byteFallback">Whether to use byte-level fallback for unknown characters</param>
    /// <exception cref="ArgumentNullException">Thrown when vocab or merges is null</exception>
    /// <exception cref="ArgumentException">Thrown when dropout is not in range [0.0, 1.0]</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native BPE model creation fails</exception>
    public BpeModel(
        IReadOnlyDictionary<string, int> vocab,
        IReadOnlyList<(string, string)> merges,
        int? cacheCapacity = null,
        float? dropout = null,
        string? unknownToken = null,
        string? continuingSubwordPrefix = null,
        string? endOfWordSuffix = null,
        bool fuseUnknown = false,
        bool byteFallback = false)
    {
        ArgumentNullException.ThrowIfNull(vocab);
        ArgumentNullException.ThrowIfNull(merges);

        if (dropout.HasValue && (dropout.Value < 0.0f || dropout.Value > 1.0f))
        {
            throw new ArgumentException("Dropout must be between 0.0 and 1.0", nameof(dropout));
        }

        // Serialize vocab to JSON
        string vocabJson = JsonSerializer.Serialize(vocab);

        // Serialize merges to string (one per line)
        string mergesStr = string.Join("\n", merges.Select(m => $"{m.Item1} {m.Item2}"));

        int status;
        _handle = NativeMethods.BpeCreate(
            vocabJson,
            mergesStr,
            (nuint)(cacheCapacity ?? 0),
            dropout ?? -1.0f,
            unknownToken,
            continuingSubwordPrefix,
            endOfWordSuffix,
            fuseUnknown,
            byteFallback,
            out status);

        if (status != 0 || _handle == IntPtr.Zero)
        {
            string error = NativeMethods.GetLastError();
            throw new InvalidOperationException($"Failed to create BPE model: {error} (status: {status})");
        }
    }

    /// <summary>
    /// Creates a BPE model from vocabulary and merges files.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary JSON file</param>
    /// <param name="mergesPath">Path to the merges text file (one merge per line)</param>
    /// <param name="cacheCapacity">Optional cache size for tokenization speed optimization (default: 10000)</param>
    /// <param name="dropout">Optional dropout probability for BPE merge operations (0.0-1.0, null = no dropout)</param>
    /// <param name="unknownToken">Token to use for unknown characters (default: null)</param>
    /// <param name="continuingSubwordPrefix">Prefix for non-initial subword tokens (e.g., "##" for BERT-style)</param>
    /// <param name="endOfWordSuffix">Suffix for end-of-word tokens (e.g., "&lt;/w&gt;")</param>
    /// <param name="fuseUnknown">Whether to fuse consecutive unknown tokens into a single token</param>
    /// <returns>A new BPE model instance loaded from the specified files</returns>
    /// <exception cref="ArgumentException">Thrown when vocabPath or mergesPath is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when vocabulary or merges file does not exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native BPE model creation fails</exception>
    /// <remarks>
    /// The vocabulary file should be a JSON object mapping tokens to integer IDs.
    /// The merges file should contain one merge per line in the format: "token1 token2"
    ///
    /// Example vocab.json:
    /// <code>
    /// {
    ///   "hello": 0,
    ///   "world": 1,
    ///   "!": 2
    /// }
    /// </code>
    ///
    /// Example merges.txt:
    /// <code>
    /// h e
    /// he l
    /// hel l
    /// hell o
    /// </code>
    /// </remarks>
    public static BpeModel FromFile(
        string vocabPath,
        string mergesPath,
        int? cacheCapacity = null,
        float? dropout = null,
        string? unknownToken = null,
        string? continuingSubwordPrefix = null,
        string? endOfWordSuffix = null,
        bool fuseUnknown = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(vocabPath);
        ArgumentException.ThrowIfNullOrEmpty(mergesPath);

        if (!File.Exists(vocabPath))
        {
            throw new FileNotFoundException($"Vocabulary file not found: {vocabPath}", vocabPath);
        }

        if (!File.Exists(mergesPath))
        {
            throw new FileNotFoundException($"Merges file not found: {mergesPath}", mergesPath);
        }

        if (dropout.HasValue && (dropout.Value < 0.0f || dropout.Value > 1.0f))
        {
            throw new ArgumentException("Dropout must be between 0.0 and 1.0", nameof(dropout));
        }

        int status;
        IntPtr handle = NativeMethods.BpeFromFile(
            vocabPath,
            mergesPath,
            (nuint)(cacheCapacity ?? 0),
            dropout ?? -1.0f,
            unknownToken,
            continuingSubwordPrefix,
            endOfWordSuffix,
            fuseUnknown,
            out status);

        if (status != 0 || handle == IntPtr.Zero)
        {
            string error = NativeMethods.GetLastError();
            throw new InvalidOperationException($"Failed to create BPE model from files: {error} (status: {status})");
        }

        return new BpeModel(handle);
    }

    /// <summary>
    /// Internal constructor for creating a BPE model from an existing native handle.
    /// </summary>
    /// <param name="handle">Native BPE model handle</param>
    private BpeModel(IntPtr handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the native handle to the BPE model.
    /// </summary>
    internal IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the BPE model.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.BpeFree(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure native resources are released.
    /// </summary>
    ~BpeModel()
    {
        Dispose();
    }
}
