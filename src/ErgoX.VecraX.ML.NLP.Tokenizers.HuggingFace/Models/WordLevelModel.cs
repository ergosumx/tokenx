using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;

/// <summary>
/// WordLevel tokenization model - the simplest tokenization approach.
/// Maps tokens directly to their IDs using a vocabulary dictionary with no subword splitting.
/// </summary>
/// <remarks>
/// WordLevel is the most basic tokenization model where each token in the vocabulary
/// maps directly to a single ID. Unlike BPE or WordPiece, it performs no subword splitting.
/// Unknown tokens are either mapped to a special unknown token ID or cause an error.
///
/// This model is useful for:
/// - Character-level tokenization
/// - Simple word-level tokenization
/// - Cases where vocabulary is known and complete
/// </remarks>
public sealed class WordLevelModel : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a WordLevel model from a vocabulary JSON file.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary JSON file (format: {"token": id})</param>
    /// <param name="unkToken">Token to use for unknown tokens (default: null - no unknown token handling)</param>
    /// <returns>A new WordLevel model instance loaded from the specified file</returns>
    /// <exception cref="ArgumentNullException">Thrown when vocabPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when vocabPath is empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when vocabulary file does not exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native WordLevel model creation fails</exception>
    /// <remarks>
    /// This method is equivalent to the Python <c>WordLevel.from_file()</c> method.
    /// The vocabulary file should be a JSON file with token-to-ID mappings.
    ///
    /// Example vocab.json:
    /// <code>
    /// {
    ///   "a": 0,
    ///   "b": 1,
    ///   "c": 2,
    ///   "[UNK]": 3
    /// }
    /// </code>
    /// </remarks>
    public static WordLevelModel FromFile(string vocabPath, string? unkToken = null)
    {
        ArgumentNullException.ThrowIfNull(vocabPath);
        ArgumentException.ThrowIfNullOrEmpty(vocabPath);

        if (!File.Exists(vocabPath))
        {
            throw new FileNotFoundException($"Vocabulary file not found: {vocabPath}", vocabPath);
        }

        int status;
        IntPtr handle = NativeMethods.WordLevelFromFile(
            vocabPath,
            unkToken,
            out status);

        if (status != 0 || handle == IntPtr.Zero)
        {
            string error = NativeMethods.GetLastError();
            throw new InvalidOperationException($"Failed to create WordLevel model from file: {error} (status: {status})");
        }

        return new WordLevelModel(handle);
    }

    /// <summary>
    /// Internal constructor that takes ownership of a native handle.
    /// </summary>
    /// <param name="handle">The native WordLevel model handle</param>
    private WordLevelModel(IntPtr handle)
    {
        _handle = handle;
        _disposed = false;
    }

    /// <summary>
    /// Gets the native handle to the WordLevel model.
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
    /// Releases the unmanaged resources used by the WordLevel model.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.WordLevelFree(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure native resources are released.
    /// </summary>
    ~WordLevelModel()
    {
        Dispose();
    }
}
