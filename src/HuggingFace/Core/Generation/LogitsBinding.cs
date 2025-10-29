namespace ErgoX.TokenX.HuggingFace.Generation;

using System;

/// <summary>
/// Represents a single logits processor or warper binding derived from a generation configuration.
/// </summary>
public sealed class LogitsBinding
{
    public LogitsBinding(string category, string kind, double value)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category must be provided.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException("Kind must be provided.", nameof(kind));
        }

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");
        }

        Category = category;
        Kind = kind;
        Value = value;
    }

    /// <summary>
    /// Gets the logical category of the binding (e.g. warper or processor).
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the specific binding kind (e.g. temperature, top_p, repetition_penalty).
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Gets the parameter value associated with the binding.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets a value indicating whether the binding represents a logits warper.
    /// </summary>
    public bool IsWarper => string.Equals(Category, LogitsBindingCategories.Warper, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the binding represents a logits processor.
    /// </summary>
    public bool IsProcessor => string.Equals(Category, LogitsBindingCategories.Processor, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Provides symbolic names for logits binding categories.
/// </summary>
public static class LogitsBindingCategories
{
    public const string Warper = "warper";
    public const string Processor = "processor";
}

