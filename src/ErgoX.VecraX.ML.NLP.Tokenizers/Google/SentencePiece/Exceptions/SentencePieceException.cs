namespace ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Exceptions;

using System;
using ErgoX.VecraX.ML.NLP.Tokenizers.Google.SentencePiece.Internal.Interop;

/// <summary>
/// Represents the status codes returned by the SentencePiece C API.
/// These codes correspond to the standard gRPC status set.
/// </summary>
public enum SentencePieceStatusCode
{
    /// <summary>Not an error; returned on success.</summary>
    Ok = 0,

    /// <summary>The operation was cancelled.</summary>
    Cancelled = 1,

    /// <summary>Unknown error.</summary>
    Unknown = 2,

    /// <summary>Invalid argument provided.</summary>
    InvalidArgument = 3,

    /// <summary>Deadline exceeded.</summary>
    DeadlineExceeded = 4,

    /// <summary>Entity not found.</summary>
    NotFound = 5,

    /// <summary>Entity already exists.</summary>
    AlreadyExists = 6,

    /// <summary>Permission denied.</summary>
    PermissionDenied = 7,

    /// <summary>Resource exhausted.</summary>
    ResourceExhausted = 8,

    /// <summary>Failed precondition.</summary>
    FailedPrecondition = 9,

    /// <summary>Operation aborted.</summary>
    Aborted = 10,

    /// <summary>Out of range.</summary>
    OutOfRange = 11,

    /// <summary>Operation not implemented.</summary>
    Unimplemented = 12,

    /// <summary>Internal error.</summary>
    Internal = 13,

    /// <summary>Service unavailable.</summary>
    Unavailable = 14,

    /// <summary>Data loss.</summary>
    DataLoss = 15,

    /// <summary>Unauthenticated.</summary>
    Unauthenticated = 16,
}

/// <summary>
/// Represents an exception thrown by the SentencePiece library.
/// Includes detailed status code information for error handling.
/// </summary>
public sealed class SentencePieceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SentencePieceException"/> class.
    /// </summary>
    /// <param name="message">The error message that describes the exception.</param>
    /// <param name="statusCode">The <see cref="SentencePieceStatusCode"/> returned by the SentencePiece C API.</param>
    public SentencePieceException(string message, SentencePieceStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the status code associated with this exception.
    /// </summary>
    public SentencePieceStatusCode StatusCode { get; }

    /// <summary>
    /// Converts a native <see cref="NativeMethods.SpcStatusCode"/> to a managed <see cref="SentencePieceStatusCode"/>.
    /// </summary>
    /// <param name="statusCode">The native status code to convert.</param>
    /// <returns>The converted status code.</returns>
    internal static SentencePieceStatusCode FromNative(NativeMethods.SpcStatusCode statusCode)
        => (SentencePieceStatusCode)(int)statusCode;
}
