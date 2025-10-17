using System.Runtime.InteropServices;

using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;

/// <summary>
/// Unigram tokenization model used by SentencePiece and similar tokenizers.
/// Uses probabilistic scores to determine the most likely tokenization of a sequence.
/// </summary>
/// <remarks>
/// Unigram is a probabilistic subword tokenization algorithm that maintains a vocabulary
/// of tokens with associated scores (log probabilities). During tokenization, it finds
/// the most likely sequence of tokens that maximize the total score.
///
/// Common models using Unigram:
/// - ALBERT (Google)
/// - T5 (Google)
/// - mBART (Facebook)
/// - XLM-RoBERTa (Facebook)
/// - Most SentencePiece models
/// </remarks>
public sealed class UnigramModel : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Represents a vocabulary item with a token and its score.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct VocabItem
    {
        /// <summary>
        /// Pointer to the UTF-8 encoded token string.
        /// </summary>
        public IntPtr Token;

        /// <summary>
        /// Score (log probability) of the token.
        /// </summary>
        public double Score;
    }

    /// <summary>
    /// Creates a new Unigram model with the specified vocabulary and scores.
    /// </summary>
    /// <param name="vocab">List of (token, score) tuples representing the vocabulary</param>
    /// <param name="unkId">Optional ID of the unknown token (default: null)</param>
    /// <param name="byteFallback">Whether to use byte fallback for unknown characters (default: false)</param>
    /// <exception cref="ArgumentNullException">Thrown when vocab is null</exception>
    /// <exception cref="ArgumentException">Thrown when vocab is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native Unigram model creation fails</exception>
    /// <remarks>
    /// This constructor is equivalent to the Python <c>Unigram(vocab, unk_id, byte_fallback)</c> constructor.
    /// The vocabulary should contain tokens with their associated scores (typically negative log probabilities).
    ///
    /// Example:
    /// <code>
    /// var vocab = new List&lt;(string, double)&gt;
    /// {
    ///     ("▁the", -2.5),
    ///     ("▁a", -3.0),
    ///     ("▁is", -3.2),
    ///     ("&lt;unk&gt;", -10.0)
    /// };
    /// using var model = new UnigramModel(vocab, unkId: 3, byteFallback: false);
    /// </code>
    /// </remarks>
    public UnigramModel(
        IReadOnlyList<(string Token, double Score)> vocab,
        int? unkId = null,
        bool byteFallback = false)
    {
        ArgumentNullException.ThrowIfNull(vocab);

        if (vocab.Count == 0)
        {
            throw new ArgumentException("Vocabulary cannot be empty", nameof(vocab));
        }

        // Convert vocab to native array
        var nativeVocab = new VocabItem[vocab.Count];
        var handles = new IntPtr[vocab.Count]; // Keep handles to prevent GC

        try
        {
            for (int i = 0; i < vocab.Count; i++)
            {
                // Allocate UTF-8 string and keep handle
                handles[i] = Marshal.StringToHGlobalAnsi(vocab[i].Token);
                nativeVocab[i] = new VocabItem
                {
                    Token = handles[i],
                    Score = vocab[i].Score
                };
            }

            // Create the model
            int status;
            IntPtr unkIdPtr = IntPtr.Zero;

            if (unkId.HasValue)
            {
                // Allocate space for usize (platform-specific: 4 bytes on 32-bit, 8 bytes on 64-bit)
                unkIdPtr = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(unkIdPtr, (IntPtr)(nuint)unkId.Value);
            }

            try
            {
                _handle = NativeMethods.UnigramNew(
                    nativeVocab,
                    (nuint)vocab.Count,
                    unkIdPtr,
                    byteFallback,
                    out status);

                if (status != 0 || _handle == IntPtr.Zero)
                {
                    string error = NativeMethods.GetLastError();
                    throw new InvalidOperationException($"Failed to create Unigram model: {error} (status: {status})");
                }
            }
            finally
            {
                if (unkIdPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(unkIdPtr);
                }
            }
        }
        finally
        {
            // Free all allocated strings
            foreach (var handle in handles)
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                }
            }
        }

        _disposed = false;
    }

    /// <summary>
    /// Gets the native handle to the Unigram model.
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
    /// Releases the unmanaged resources used by the Unigram model.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.UnigramFree(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure native resources are released.
    /// </summary>
    ~UnigramModel()
    {
        Dispose();
    }
}
