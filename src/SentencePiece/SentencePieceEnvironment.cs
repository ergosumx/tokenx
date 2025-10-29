namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;

/// <summary>
/// Provides global configuration and environment settings for the SentencePiece library.
/// Controls random number generation, logging levels, and data directories.
/// </summary>
public static class SentencePieceEnvironment
{
    /// <summary>
    /// Sets the seed for the random number generator used by SentencePiece.
    /// Useful for reproducible results in sampling-based operations.
    /// </summary>
    /// <param name="seed">The random seed value to set.</param>
    public static void SetRandomGeneratorSeed(uint seed)
    {
        NativeMethods.spc_set_random_generator_seed(seed);
    }

    /// <summary>
    /// Sets the minimum log level for SentencePiece logging output.
    /// Higher values reduce the verbosity of logging.
    /// </summary>
    /// <param name="level">The minimum log level (0=most verbose, higher=less verbose).</param>
    public static void SetMinLogLevel(int level)
    {
        NativeMethods.spc_set_min_log_level(level);
    }

    /// <summary>
    /// Sets the data directory path for SentencePiece resources.
    /// This is used to locate model files and other resources.
    /// </summary>
    /// <param name="path">The directory path to set.</param>
    /// <exception cref="ArgumentException">Thrown when path is null, empty, or whitespace-only.</exception>
    public static void SetDataDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided.", nameof(path));
        }

        using var value = new InteropUtilities.NativeUtf8(path);
        NativeMethods.spc_set_data_dir(value.View);
    }
}
