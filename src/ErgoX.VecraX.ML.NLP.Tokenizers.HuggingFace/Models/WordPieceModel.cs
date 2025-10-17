using ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Internal.Interop;

namespace ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Models;

/// <summary>
/// WordPiece tokenization model used by BERT, DistilBERT, ELECTRA, and similar transformers.
/// WordPiece segments text by greedily selecting the longest matching subword from a fixed vocabulary.
/// </summary>
/// <remarks>
/// WordPiece is a subword tokenization algorithm that uses a fixed vocabulary of tokens learned
/// during training. It tokenizes by greedily selecting the longest matching subword at each step,
/// using a special prefix (typically "##") to mark continuation subwords that are not at the
/// beginning of a word.
///
/// Common models using WordPiece:
/// - BERT (Google)
/// - DistilBERT (HuggingFace)
/// - ELECTRA (Google/Stanford)
/// - ALBERT (Google)
/// </remarks>
public sealed class WordPieceModel : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a WordPiece model from a vocabulary file.
    /// </summary>
    /// <param name="vocabPath">Path to the vocabulary text file (one token per line)</param>
    /// <param name="unkToken">Token to use for unknown characters (default: "[UNK]")</param>
    /// <param name="maxInputCharsPerWord">Maximum number of characters per word (default: 100)</param>
    /// <param name="continuingSubwordPrefix">Prefix for non-initial subword tokens (default: "##")</param>
    /// <returns>A new WordPiece model instance loaded from the specified file</returns>
    /// <exception cref="ArgumentNullException">Thrown when vocabPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when vocabPath is empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when vocabulary file does not exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the native WordPiece model creation fails</exception>
    /// <remarks>
    /// This method is equivalent to the Python <c>WordPiece.from_file()</c> method.
    /// The vocabulary file should be a text file with one token per line, where the line number
    /// corresponds to the token ID (0-based).
    ///
    /// Example vocab.txt:
    /// <code>
    /// [PAD]
    /// [UNK]
    /// [CLS]
    /// [SEP]
    /// the
    /// ##ing
    /// </code>
    /// </remarks>
    public static WordPieceModel FromFile(
        string vocabPath,
        string? unkToken = null,
        int maxInputCharsPerWord = 100,
        string? continuingSubwordPrefix = null)
    {
        ArgumentNullException.ThrowIfNull(vocabPath);
        ArgumentException.ThrowIfNullOrEmpty(vocabPath);

        if (!File.Exists(vocabPath))
        {
            throw new FileNotFoundException($"Vocabulary file not found: {vocabPath}", vocabPath);
        }

        if (maxInputCharsPerWord <= 0)
        {
            throw new ArgumentException("Max input characters per word must be positive", nameof(maxInputCharsPerWord));
        }

        int status;
        IntPtr handle = NativeMethods.WordPieceFromFile(
            vocabPath,
            unkToken,
            (nuint)maxInputCharsPerWord,
            continuingSubwordPrefix,
            out status);

        if (status != 0 || handle == IntPtr.Zero)
        {
            string error = NativeMethods.GetLastError();
            throw new InvalidOperationException($"Failed to create WordPiece model from file: {error} (status: {status})");
        }

        return new WordPieceModel(handle);
    }

    /// <summary>
    /// Internal constructor that takes ownership of a native handle.
    /// </summary>
    /// <param name="handle">The native WordPiece model handle</param>
    private WordPieceModel(IntPtr handle)
    {
        _handle = handle;
        _disposed = false;
    }

    /// <summary>
    /// Gets the native handle to the WordPiece model.
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
    /// Releases the unmanaged resources used by the WordPiece model.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.WordPieceFree(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure native resources are released.
    /// </summary>
    ~WordPieceModel()
    {
        Dispose();
    }
}
