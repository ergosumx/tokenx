namespace ErgoX.VecraX.ML.NLP.Tokenizers.Tiktoken;

using System;

/// <summary>
/// Represents errors raised by the native TikToken bridge.
/// </summary>
public class TiktokenInteropException : InvalidOperationException
{
    public TiktokenInteropException()
    {
    }

    public TiktokenInteropException(string message)
        : base(message)
    {
    }

    public TiktokenInteropException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
