namespace ErgoX.TokenX.HuggingFace.Options;

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

/// <summary>
/// Represents runtime overrides that can be applied on top of defaults loaded from generation_config.json.
/// </summary>
public sealed class GenerationOptions
{
    private bool _temperatureSpecified;
    private double? _temperature;

    private bool _topPSpecified;
    private double? _topP;

    private bool _topKSpecified;
    private int? _topK;

    private bool _repetitionPenaltySpecified;
    private double? _repetitionPenalty;

    private bool _maxNewTokensSpecified;
    private int? _maxNewTokens;

    private bool _minNewTokensSpecified;
    private int? _minNewTokens;

    private bool _doSampleSpecified;
    private bool? _doSample;

    private bool _numBeamsSpecified;
    private int? _numBeams;

    private bool _stopSequencesSpecified;
    private IReadOnlyList<string>? _stopSequences;

    /// <summary>
    /// Gets a mutable dictionary of additional parameters that should be merged with the defaults.
    /// </summary>
    public IDictionary<string, JsonNode?> AdditionalParameters { get; } = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the sampling temperature. Setting the property marks it as an explicit override.
    /// Assign <c>null</c> to remove the value during merging.
    /// </summary>
    public double? Temperature
    {
        get => _temperature;
        set
        {
            _temperature = value;
            _temperatureSpecified = true;
        }
    }

    internal bool TemperatureSpecified => _temperatureSpecified;

    /// <summary>
    /// Gets or sets the nucleus sampling probability. Assign <c>null</c> to clear the default.
    /// </summary>
    public double? TopP
    {
        get => _topP;
        set
        {
            _topP = value;
            _topPSpecified = true;
        }
    }

    internal bool TopPSpecified => _topPSpecified;

    /// <summary>
    /// Gets or sets the top-k sampling parameter. Assign <c>null</c> to clear the default.
    /// </summary>
    public int? TopK
    {
        get => _topK;
        set
        {
            _topK = value;
            _topKSpecified = true;
        }
    }

    internal bool TopKSpecified => _topKSpecified;

    /// <summary>
    /// Gets or sets the repetition penalty.
    /// </summary>
    public double? RepetitionPenalty
    {
        get => _repetitionPenalty;
        set
        {
            _repetitionPenalty = value;
            _repetitionPenaltySpecified = true;
        }
    }

    internal bool RepetitionPenaltySpecified => _repetitionPenaltySpecified;

    /// <summary>
    /// Gets or sets the maximum number of new tokens to generate.
    /// </summary>
    public int? MaxNewTokens
    {
        get => _maxNewTokens;
        set
        {
            _maxNewTokens = value;
            _maxNewTokensSpecified = true;
        }
    }

    internal bool MaxNewTokensSpecified => _maxNewTokensSpecified;

    /// <summary>
    /// Gets or sets the minimum number of tokens to generate.
    /// </summary>
    public int? MinNewTokens
    {
        get => _minNewTokens;
        set
        {
            _minNewTokens = value;
            _minNewTokensSpecified = true;
        }
    }

    internal bool MinNewTokensSpecified => _minNewTokensSpecified;

    /// <summary>
    /// Gets or sets whether sampling should be used.
    /// </summary>
    public bool? DoSample
    {
        get => _doSample;
        set
        {
            _doSample = value;
            _doSampleSpecified = true;
        }
    }

    internal bool DoSampleSpecified => _doSampleSpecified;

    /// <summary>
    /// Gets or sets the number of beams used during beam search.
    /// </summary>
    public int? NumBeams
    {
        get => _numBeams;
        set
        {
            _numBeams = value;
            _numBeamsSpecified = true;
        }
    }

    internal bool NumBeamsSpecified => _numBeamsSpecified;

    /// <summary>
    /// Gets or sets the stop sequences. Assign <c>null</c> to remove the default sequences.
    /// </summary>
    public IReadOnlyList<string>? StopSequences
    {
        get => _stopSequences;
        set
        {
            _stopSequences = value;
            _stopSequencesSpecified = true;
        }
    }

    internal bool StopSequencesSpecified => _stopSequencesSpecified;
}

