namespace Examples.HuggingFace.WhisperTinyConsole;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ErgoX.TokenX.HuggingFace;
using ErgoX.TokenX.HuggingFace.Options;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

/// <summary>
/// Whisper Tiny console: Speech-to-text using encoder-decoder ONNX models with greedy decoding.
/// 
/// Pipeline:
/// 1. Audio Loading: MP3 decode → PCM samples
/// 2. Audio Preprocessing: Resample to 16 kHz (Whisper requirement)
/// 3. Feature Extraction: FFT → Mel-frequency warping → Log scaling → Log-mel spectrogram (80×1500)
/// 4. Encoder Inference: Spectrogram → Audio embeddings (1, 1500, 384)
/// 5. Greedy Decoding: Decoder generates tokens iteratively (up to 128 tokens)
/// 6. Text Output: Decode token IDs → text (skip special tokens)
/// 
/// Quantization: QINT8 (4× smaller, 2-5% accuracy loss)
/// Performance: ~1-2 seconds per 30-second audio file (CPU, quantized)
/// 
/// Key Concepts:
/// - Encoder-decoder architecture: Common for sequence-to-sequence tasks
/// - Greedy decoding: Fast but lower quality than beam search
/// - Special tokens: Whisper uses language/task tokens for control
/// </summary>
internal static class Program
{
    private const string ModelId = "whisper-tiny";
    private const string EncoderModelName = "encoder_model_quantized.onnx";
    private const string DecoderModelName = "decoder_model_quantized.onnx";
    
    // Audio constants: Whisper expects 16 kHz mono PCM
    private const int SampleRate = 16_000;
    private const int ChunkLengthSeconds = 30;
    private const int MaxAudioSamples = SampleRate * ChunkLengthSeconds;
    
    // Spectrogram constants (STFT parameters)
    private const int Nfft = 400;              // FFT size (25 ms @ 16 kHz)
    private const int HopLength = 160;         // Window hop (10 ms @ 16 kHz)
    private const int MelBins = 80;            // Mel-frequency bins (human-perception scale)
    private const int Frames = MaxAudioSamples / HopLength;  // 3000 frames for 30 sec
    
    // Decoding constants
    private const int MaxDecoderTokens = 128;
    private const double LogOffset = 1e-10;
    
    // Pre-computed audio window (reduces repeated computation)
    private static readonly double[] HannWindow = Window.Hann(Nfft);
    
    // Pre-computed mel filterbank (triangular filters in mel space)
    private static readonly double[][] MelFilterBank = CreateMelFilterBank();

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Resolve model and audio directories
        var modelDirectory = ResolveModelDirectory(ModelId);
        var audioDirectory = ResolveAudioDirectory();
        
        // Load all MP3 files (sorted for consistency)
        var audioFiles = Directory.EnumerateFiles(audioDirectory, "*.mp3", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        if (audioFiles.Length == 0)
        {
            Console.WriteLine($"No audio files were found under '{audioDirectory}'.");
            return;
        }

        // Load tokenizer (for special tokens and decoding)
        using var tokenizer = AutoTokenizer.Load(modelDirectory, new AutoTokenizerLoadOptions
        {
            ApplyTokenizerDefaults = true,
            LoadGenerationConfig = true
        });

        // Load ONNX encoder and decoder models (quantized QINT8)
        using var encoderSession = CreateSession(Path.Combine(modelDirectory, EncoderModelName));
        using var decoderSession = CreateSession(Path.Combine(modelDirectory, DecoderModelName));

        Console.WriteLine($"Loaded Whisper Tiny assets from: {modelDirectory}");
        Console.WriteLine($"Scanning audio inputs from: {audioDirectory}");
        Console.WriteLine();

        // Transcribe each audio file
        foreach (var filePath in audioFiles)
        {
            Console.WriteLine($"Transcribing '{Path.GetFileName(filePath)}':");

            // Step 1: Load and preprocess audio
            var audio = LoadAudioSamples(filePath);
            
            // Step 2: Extract features (log-mel spectrogram)
            var features = ComputeLogMelSpectrogram(audio);
            
            // Step 3: Encoder inference (audio → embeddings)
            var encoderOutput = RunEncoder(encoderSession, features);
            Console.WriteLine($"Encoder hidden shape: {string.Join(", ", encoderOutput.Dimensions)}");
            
            // Step 4: Greedy decoding (embeddings + decoder → text)
            var transcription = DecodeGreedy(decoderSession, tokenizer, encoderOutput);

            Console.WriteLine(string.IsNullOrWhiteSpace(transcription)
                ? "  (no transcription produced)"
                : $"  {transcription}");
            Console.WriteLine(new string('-', 72));
        }
    }

    private static InferenceSession CreateSession(string modelPath)
    {
        // Load ONNX model and configure session
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file '{modelPath}' was not found.", modelPath);
        }

        var options = new SessionOptions
        {
            LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        try
        {
            return new InferenceSession(modelPath, options);
        }
        finally
        {
            options.Dispose();
        }
    }

    private static EncoderOutput RunEncoder(InferenceSession session, DenseTensor<float> features)
    {
        // Run encoder on mel-spectrogram features
        // Input: features (1, 80, 1500) = (batch, mel_bins, frames)
        // Output: last_hidden_state (1, 1500, 384) = (batch, frames, embedding_dim)
        var inputs = new[] { NamedOnnxValue.CreateFromTensor("input_features", features) };
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = session.Run(inputs);
            using var hidden = GetOnnxOutput(results, "last_hidden_state");
            var tensor = hidden.AsTensor<float>();
            var dims = tensor.Dimensions.ToArray();
            var data = tensor.ToArray();
            return new EncoderOutput(data, dims);
        }
        finally
        {
            results?.Dispose();

            foreach (var input in inputs)
            {
                if (input is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private static string DecodeGreedy(InferenceSession session, AutoTokenizer tokenizer, EncoderOutput encoderOutput)
    {
        // Greedy decoding: Generate tokens one at a time, always pick highest logit
        // Initialize with prompt tokens:
        // - <|startoftranscript|>: Whisper start token
        // - <|en|>: Language token (English)
        // - <|transcribe|>: Task token (transcription, not translation)
        // - <|notimestamps|>: Disable timestamps in output
        var promptIds = GetPromptTokenIds(tokenizer);
        var generated = new List<long>(promptIds);
        var endTokenId = GetRequiredTokenId(tokenizer, "<|endoftext|>");
        var hiddenTensor = new DenseTensor<float>(encoderOutput.HiddenStates, encoderOutput.Dimensions);

        // Greedy loop: Repeat up to MaxDecoderTokens times
        for (var step = 0; step < MaxDecoderTokens; step++)
        {
            // Get logits for next token from decoder
            var logits = RunDecoderStep(session, hiddenTensor, generated);
            
            // Select token with highest logit (greedy = argmax)
            var nextToken = SelectNextToken(logits);
            generated.Add(nextToken);

            // Stop if we hit end-of-text token
            if (nextToken == endTokenId)
            {
                break;
            }
        }

        // Decode generated token IDs to text
        // Skip prompt tokens and special tokens
        var decodedIds = generated
            .Skip(promptIds.Count)  // Remove prompt tokens
            .TakeWhile(tokenId => tokenId != endTokenId)  // Stop at end token
            .Select(tokenId => (int)tokenId)
            .ToArray();

        if (decodedIds.Length == 0)
        {
            return string.Empty;
        }

        return tokenizer.Tokenizer.Decode(decodedIds, skipSpecialTokens: true).Trim();
    }

    private static DenseTensor<float> RunDecoderStep(InferenceSession session, DenseTensor<float> hiddenTensor, IReadOnlyList<long> tokens)
    {
        // Run decoder for one step
        // Input: current token sequence + encoder hidden states
        // Output: logits (1, sequence_length, vocab_size) for all positions
        var tokenArray = tokens.ToArray();
        var inputTensor = new DenseTensor<long>(tokenArray, new[] { 1, tokenArray.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
            NamedOnnxValue.CreateFromTensor("encoder_hidden_states", hiddenTensor)
        };

        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = session.Run(inputs);
            DisposableNamedOnnxValue? logitsValue = null;

            // Find logits output
            foreach (var output in results)
            {
                if (string.Equals(output.Name, "logits", StringComparison.Ordinal))
                {
                    logitsValue = output;
                    continue;
                }

                output.Dispose();
            }

            if (logitsValue is null)
            {
                throw new InvalidOperationException("Decoder did not return logits.");
            }

            var logitsTensor = logitsValue.AsTensor<float>();
            var dims = logitsTensor.Dimensions.ToArray();
            var logitsData = logitsTensor.ToArray();
            logitsValue.Dispose();

            return new DenseTensor<float>(logitsData, dims);
        }
        finally
        {
            results?.Dispose();

            foreach (var input in inputs)
            {
                if (input is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private static long SelectNextToken(Tensor<float> logits)
    {
        // Extract next token using greedy selection (argmax)
        // Logits shape: (1, sequence_length, vocab_size)
        // We want logits for the LAST position in sequence
        var dims = logits.Dimensions;
        if (dims.Length != 3)
        {
            throw new InvalidOperationException($"Unexpected logits rank {dims.Length}. Expected 3D tensor.");
        }

        var vocabSize = dims[2];
        var sequenceLength = dims[1];
        if (sequenceLength <= 0)
        {
            throw new InvalidOperationException("Decoder produced no time steps.");
        }
        
        var data = logits.ToArray();
        // Offset to last position: vocab_size * (sequenceLength - 1)
        var offset = vocabSize * (sequenceLength - 1);
        
        // Find index with max logit
        var maxValue = float.NegativeInfinity;
        var maxIndex = 0;
        for (var index = 0; index < vocabSize; index++)
        {
            var value = data[offset + index];
            if (value > maxValue)
            {
                maxValue = value;
                maxIndex = index;
            }
        }

        return maxIndex;
    }

    private static IReadOnlyList<long> GetPromptTokenIds(AutoTokenizer tokenizer)
    {
        // Whisper prompt tokens signal task and language to decoder
        var tokenIds = new List<long>
        {
            GetRequiredTokenId(tokenizer, "<|startoftranscript|>"),
            GetRequiredTokenId(tokenizer, "<|en|>"),
            GetRequiredTokenId(tokenizer, "<|transcribe|>"),
            GetRequiredTokenId(tokenizer, "<|notimestamps|>")
        };

        return tokenIds;
    }

    private static long GetRequiredTokenId(AutoTokenizer tokenizer, string token)
    {
        // Look up token ID in vocabulary
        var id = tokenizer.Tokenizer.TokenToId(token);
        if (id is null)
        {
            throw new InvalidOperationException($"Tokenizer does not define token '{token}'.");
        }

        return id.Value;
    }

    private static DenseTensor<float> ComputeLogMelSpectrogram(float[] audio)
    {
        // Extract log-mel spectrogram from audio samples
        // Output shape: (1, 80, 1500) = (batch, mel_bins, frames)
        // Each frame: 10 ms of audio, analyzed in 25 ms windows with 50% overlap
        
        // Mirror-pad around frame centers to emulate torch.stft(center=True, pad_mode="reflect")
        var melFrames = new double[MelBins * Frames];
        var tempSpectrum = new double[(Nfft / 2) + 1];
        var fftBuffer = new Complex[Nfft];

        // Process each frame
        for (var frame = 0; frame < Frames; frame++)
        {
            // Frame samples: centered at frame * HopLength, ±Nfft/2 for windowing
            var start = frame * HopLength - (Nfft / 2);
            
            // Fill FFT buffer with windowed audio samples
            // Hann window: smooth fade in/out to reduce spectral leakage
            for (var bin = 0; bin < Nfft; bin++)
            {
                var sample = FetchSampleWithReflection(audio, start + bin);
                fftBuffer[bin] = new Complex(sample * HannWindow[bin], 0.0);
            }

            // Forward FFT: time domain → frequency domain
            Fourier.Forward(fftBuffer, FourierOptions.Matlab);

            // Power spectrum: |X[k]|^2 = real^2 + imag^2
            for (var bin = 0; bin < tempSpectrum.Length; bin++)
            {
                var value = fftBuffer[bin];
                tempSpectrum[bin] = (value.Real * value.Real) + (value.Imaginary * value.Imaginary);
            }

            // Apply mel-frequency filterbank
            // Converts 201 linear frequency bins to 80 mel-scale bins
            // Mimics human hearing (logarithmic above ~1000 Hz)
            for (var mel = 0; mel < MelBins; mel++)
            {
                var filter = MelFilterBank[mel];
                double sum = 0.0;
                for (var bin = 0; bin < filter.Length; bin++)
                {
                    sum += filter[bin] * tempSpectrum[bin];
                }

                // Log scaling: log(power + epsilon) to avoid log(0)
                sum = Math.Max(sum, LogOffset);
                var logMel = Math.Log10(sum);
                melFrames[(mel * Frames) + frame] = logMel;
            }
        }

        // Normalize to [-4, 4] range (Whisper convention)
        var max = melFrames.Max();
        for (var index = 0; index < melFrames.Length; index++)
        {
            var value = melFrames[index];
            value = Math.Max(value, max - 8.0);  // Clip to dynamic range
            value = (value + 4.0) / 4.0;         // Normalize to [0, 2]
            melFrames[index] = value;
        }

        // Convert to float and reshape to (1, 80, frames)
        var features = new float[1 * MelBins * Frames];
        for (var index = 0; index < melFrames.Length; index++)
        {
            features[index] = (float)melFrames[index];
        }

        return new DenseTensor<float>(features, new[] { 1, MelBins, Frames });
    }

    private static double[][] CreateMelFilterBank()
    {
        // Create triangular filters in mel-frequency space
        // Maps 201 linear frequency bins to 80 mel-scale bins
        var result = new double[MelBins][];
        
        // Mel scale: mel = 2595 * log10(1 + hz/700)
        // Human perception: logarithmic at high frequencies
        var melMin = HzToMel(0.0);
        var melMax = HzToMel(SampleRate / 2.0);
        
        // Generate (MelBins + 2) equally-spaced points in mel space
        var melPoints = new double[MelBins + 2];
        for (var index = 0; index < melPoints.Length; index++)
        {
            melPoints[index] = melMin + ((melMax - melMin) * index / (MelBins + 1));
        }

        // Convert mel points back to Hz
        var hzPoints = melPoints.Select(MelToHz).ToArray();
        
        // Convert Hz to FFT bin indices
        var fftSize = (Nfft / 2) + 1;
        var binPoints = hzPoints
            .Select(hz => (int)Math.Floor(((Nfft + 1) * hz) / SampleRate))
            .Select(bin => Math.Clamp(bin, 0, fftSize - 1))
            .ToArray();

        // Create triangular filters
        for (var mel = 0; mel < MelBins; mel++)
        {
            var filter = new double[fftSize];
            var left = binPoints[mel];
            var center = Math.Max(binPoints[mel + 1], left + 1);
            var right = Math.Max(binPoints[mel + 2], center + 1);

            // Rising slope: left to center
            for (var bin = left; bin < center; bin++)
            {
                filter[bin] = (bin - left) / (double)(center - left);
            }

            // Falling slope: center to right
            for (var bin = center; bin < right; bin++)
            {
                filter[bin] = (right - bin) / (double)(right - center);
            }

            filter[^1] = 0.0;  // Ensure last bin is 0
            result[mel] = filter;
        }

        return result;
    }

    private static double HzToMel(double hz)
        => 2595.0 * Math.Log10(1.0 + (hz / 700.0));

    private static double MelToHz(double mel)
        => 700.0 * (Math.Pow(10.0, mel / 2595.0) - 1.0);

    private static float FetchSampleWithReflection(float[] audio, int index)
    {
        // Reflect out-of-bounds indices to handle frame centering
        // Examples: -1 → 1, -2 → 2, length → length-2, length+1 → length-3
        var length = audio.Length;
        if (length == 0)
        {
            return 0.0f;
        }

        if (length == 1)
        {
            return audio[0];
        }

        while (true)
        {
            if (index < 0)
            {
                index = -index;  // Reflect: -i → i
                continue;
            }

            if (index < length)
            {
                return audio[index];
            }

            // Reflect: i >= length → (2*length - 2 - i)
            index = (length - 1) - (index - length + 1);
        }
    }

    private static float[] LoadAudioSamples(string filePath)
    {
        // Load MP3 file and resample to 16 kHz mono PCM
        using var reader = new AudioFileReader(filePath);
        ISampleProvider sampleProvider = reader;

        // Convert stereo to mono if needed
        if (reader.WaveFormat.Channels > 1)
        {
            sampleProvider = new StereoToMonoSampleProvider(reader)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };
        }

        // Resample to 16 kHz (Whisper requirement)
        var resampler = new WdlResamplingSampleProvider(sampleProvider, SampleRate);
        var target = new float[MaxAudioSamples];
        var buffer = new float[SampleRate];
        var written = 0;

        // Read in chunks
        while (written < MaxAudioSamples)
        {
            var needed = Math.Min(buffer.Length, MaxAudioSamples - written);
            var read = resampler.Read(buffer, 0, needed);
            if (read == 0)
            {
                break;
            }

            Array.Copy(buffer, 0, target, written, read);
            written += read;
        }

        // Pad with zeros if audio is shorter than MaxAudioSamples
        if (written < MaxAudioSamples)
        {
            Array.Clear(target, written, MaxAudioSamples - written);
        }

        return target;
    }

    private static string ResolveModelDirectory(string modelId)
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".models", modelId);
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Model directory '{fullPath}' was not found.");
        }

        return fullPath;
    }

    private static string ResolveAudioDirectory()
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".data", "wav");
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Audio directory '{fullPath}' was not found.");
        }

        return fullPath;
    }

    private static DisposableNamedOnnxValue GetOnnxOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, string name)
    {
        // Find output by name, or return first output if not found
        foreach (var result in results)
        {
            if (string.Equals(result.Name, name, StringComparison.Ordinal))
            {
                return result;
            }
        }

        if (!results.Any())
        {
            throw new InvalidOperationException("The ONNX session returned no outputs.");
        }

        return results.First();
    }

    private sealed record EncoderOutput(float[] HiddenStates, int[] Dimensions);

    private static InferenceSession CreateSession(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file '{modelPath}' was not found.", modelPath);
        }

        var options = new SessionOptions
        {
            LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        try
        {
            return new InferenceSession(modelPath, options);
        }
        finally
        {
            options.Dispose();
        }
    }

    private static EncoderOutput RunEncoder(InferenceSession session, DenseTensor<float> features)
    {
        var inputs = new[] { NamedOnnxValue.CreateFromTensor("input_features", features) };
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = session.Run(inputs);
            using var hidden = GetOnnxOutput(results, "last_hidden_state");
            var tensor = hidden.AsTensor<float>();
            var dims = tensor.Dimensions.ToArray();
            var data = tensor.ToArray();
            return new EncoderOutput(data, dims);
        }
        finally
        {
            results?.Dispose();

            foreach (var input in inputs)
            {
                if (input is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private static string DecodeGreedy(InferenceSession session, AutoTokenizer tokenizer, EncoderOutput encoderOutput)
    {
        var promptIds = GetPromptTokenIds(tokenizer);
        var generated = new List<long>(promptIds);
        var endTokenId = GetRequiredTokenId(tokenizer, "<|endoftext|>");
        var hiddenTensor = new DenseTensor<float>(encoderOutput.HiddenStates, encoderOutput.Dimensions);

        for (var step = 0; step < MaxDecoderTokens; step++)
        {
            var logits = RunDecoderStep(session, hiddenTensor, generated);
            var nextToken = SelectNextToken(logits);
            generated.Add(nextToken);

            if (nextToken == endTokenId)
            {
                break;
            }
        }

        var decodedIds = generated
            .Skip(promptIds.Count)
            .TakeWhile(tokenId => tokenId != endTokenId)
            .Select(tokenId => (int)tokenId)
            .ToArray();

        if (decodedIds.Length == 0)
        {
            return string.Empty;
        }

        return tokenizer.Tokenizer.Decode(decodedIds, skipSpecialTokens: true).Trim();
    }

    private static DenseTensor<float> RunDecoderStep(InferenceSession session, DenseTensor<float> hiddenTensor, IReadOnlyList<long> tokens)
    {
    var tokenArray = tokens.ToArray();
    var inputTensor = new DenseTensor<long>(tokenArray, new[] { 1, tokenArray.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
            NamedOnnxValue.CreateFromTensor("encoder_hidden_states", hiddenTensor)
        };

        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = session.Run(inputs);
            DisposableNamedOnnxValue? logitsValue = null;

            foreach (var output in results)
            {
                if (string.Equals(output.Name, "logits", StringComparison.Ordinal))
                {
                    logitsValue = output;
                    continue;
                }

                output.Dispose();
            }

            if (logitsValue is null)
            {
                throw new InvalidOperationException("Decoder did not return logits.");
            }

            var logitsTensor = logitsValue.AsTensor<float>();
            var dims = logitsTensor.Dimensions.ToArray();
            var logitsData = logitsTensor.ToArray();
            logitsValue.Dispose();

            return new DenseTensor<float>(logitsData, dims);
        }
        finally
        {
            results?.Dispose();

            foreach (var input in inputs)
            {
                if (input is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }

    private static long SelectNextToken(Tensor<float> logits)
    {
        var dims = logits.Dimensions;
        if (dims.Length != 3)
        {
            throw new InvalidOperationException($"Unexpected logits rank {dims.Length}. Expected 3D tensor.");
        }

        var vocabSize = dims[2];
        var sequenceLength = dims[1];
        if (sequenceLength <= 0)
        {
            throw new InvalidOperationException("Decoder produced no time steps.");
        }
        var data = logits.ToArray();
        var offset = vocabSize * (sequenceLength - 1);
        var maxValue = float.NegativeInfinity;
        var maxIndex = 0;
        for (var index = 0; index < vocabSize; index++)
        {
            var value = data[offset + index];
            if (value > maxValue)
            {
                maxValue = value;
                maxIndex = index;
            }
        }

        return maxIndex;
    }

    private static IReadOnlyList<long> GetPromptTokenIds(AutoTokenizer tokenizer)
    {
        var tokenIds = new List<long>
        {
            GetRequiredTokenId(tokenizer, "<|startoftranscript|>"),
            GetRequiredTokenId(tokenizer, "<|en|>"),
            GetRequiredTokenId(tokenizer, "<|transcribe|>"),
            GetRequiredTokenId(tokenizer, "<|notimestamps|>")
        };

        return tokenIds;
    }

    private static long GetRequiredTokenId(AutoTokenizer tokenizer, string token)
    {
        var id = tokenizer.Tokenizer.TokenToId(token);
        if (id is null)
        {
            throw new InvalidOperationException($"Tokenizer does not define token '{token}'.");
        }

        return id.Value;
    }

    private static DenseTensor<float> ComputeLogMelSpectrogram(float[] audio)
    {
        // Mirror-pad around frame centers to emulate torch.stft(center=True, pad_mode="reflect").
        var melFrames = new double[MelBins * Frames];
        var tempSpectrum = new double[(Nfft / 2) + 1];
        var fftBuffer = new Complex[Nfft];

        for (var frame = 0; frame < Frames; frame++)
        {
            var start = frame * HopLength - (Nfft / 2);
            for (var bin = 0; bin < Nfft; bin++)
            {
                var sample = FetchSampleWithReflection(audio, start + bin);
                fftBuffer[bin] = new Complex(sample * HannWindow[bin], 0.0);
            }

            Fourier.Forward(fftBuffer, FourierOptions.Matlab);

            for (var bin = 0; bin < tempSpectrum.Length; bin++)
            {
                var value = fftBuffer[bin];
                tempSpectrum[bin] = (value.Real * value.Real) + (value.Imaginary * value.Imaginary);
            }

            for (var mel = 0; mel < MelBins; mel++)
            {
                var filter = MelFilterBank[mel];
                double sum = 0.0;
                for (var bin = 0; bin < filter.Length; bin++)
                {
                    sum += filter[bin] * tempSpectrum[bin];
                }

                sum = Math.Max(sum, LogOffset);
                var logMel = Math.Log10(sum);
                melFrames[(mel * Frames) + frame] = logMel;
            }
        }

        var max = melFrames.Max();
        for (var index = 0; index < melFrames.Length; index++)
        {
            var value = melFrames[index];
            value = Math.Max(value, max - 8.0);
            value = (value + 4.0) / 4.0;
            melFrames[index] = value;
        }

        var features = new float[1 * MelBins * Frames];
        for (var index = 0; index < melFrames.Length; index++)
        {
            features[index] = (float)melFrames[index];
        }

        return new DenseTensor<float>(features, new[] { 1, MelBins, Frames });
    }

    private static double[][] CreateMelFilterBank()
    {
        var result = new double[MelBins][];
        var melMin = HzToMel(0.0);
        var melMax = HzToMel(SampleRate / 2.0);
        var melPoints = new double[MelBins + 2];
        for (var index = 0; index < melPoints.Length; index++)
        {
            melPoints[index] = melMin + ((melMax - melMin) * index / (MelBins + 1));
        }

        var hzPoints = melPoints.Select(MelToHz).ToArray();
        var fftSize = (Nfft / 2) + 1;
        var binPoints = hzPoints
            .Select(hz => (int)Math.Floor(((Nfft + 1) * hz) / SampleRate))
            .Select(bin => Math.Clamp(bin, 0, fftSize - 1))
            .ToArray();

        for (var mel = 0; mel < MelBins; mel++)
        {
            var filter = new double[fftSize];
            var left = binPoints[mel];
            var center = Math.Max(binPoints[mel + 1], left + 1);
            var right = Math.Max(binPoints[mel + 2], center + 1);

            for (var bin = left; bin < center; bin++)
            {
                filter[bin] = (bin - left) / (double)(center - left);
            }

            for (var bin = center; bin < right; bin++)
            {
                filter[bin] = (right - bin) / (double)(right - center);
            }

            filter[^1] = 0.0;
            result[mel] = filter;
        }

        return result;
    }

    private static double HzToMel(double hz)
        => 2595.0 * Math.Log10(1.0 + (hz / 700.0));

    private static double MelToHz(double mel)
        => 700.0 * (Math.Pow(10.0, mel / 2595.0) - 1.0);

    private static float FetchSampleWithReflection(float[] audio, int index)
    {
        var length = audio.Length;
        if (length == 0)
        {
            return 0.0f;
        }

        if (length == 1)
        {
            return audio[0];
        }

        while (true)
        {
            if (index < 0)
            {
                index = -index;
                continue;
            }

            if (index < length)
            {
                return audio[index];
            }

            index = (length - 1) - (index - length + 1);
        }
    }

    private static float[] LoadAudioSamples(string filePath)
    {
        using var reader = new AudioFileReader(filePath);
        ISampleProvider sampleProvider = reader;

        if (reader.WaveFormat.Channels > 1)
        {
            sampleProvider = new StereoToMonoSampleProvider(reader)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };
        }

        var resampler = new WdlResamplingSampleProvider(sampleProvider, SampleRate);
        var target = new float[MaxAudioSamples];
        var buffer = new float[SampleRate];
        var written = 0;

        while (written < MaxAudioSamples)
        {
            var needed = Math.Min(buffer.Length, MaxAudioSamples - written);
            var read = resampler.Read(buffer, 0, needed);
            if (read == 0)
            {
                break;
            }

            Array.Copy(buffer, 0, target, written, read);
            written += read;
        }

        if (written < MaxAudioSamples)
        {
            Array.Clear(target, written, MaxAudioSamples - written);
        }

        return target;
    }

    private static string ResolveModelDirectory(string modelId)
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".models", modelId);
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Model directory '{fullPath}' was not found.");
        }

        return fullPath;
    }

    private static string ResolveAudioDirectory()
    {
        var relative = Path.Combine("..", "..", "..", "..", "..", ".data", "wav");
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Audio directory '{fullPath}' was not found.");
        }

        return fullPath;
    }

    private static DisposableNamedOnnxValue GetOnnxOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, string name)
    {
        foreach (var result in results)
        {
            if (string.Equals(result.Name, name, StringComparison.Ordinal))
            {
                return result;
            }
        }

        if (!results.Any())
        {
            throw new InvalidOperationException("The ONNX session returned no outputs.");
        }

        return results.First();
    }

    private sealed record EncoderOutput(float[] HiddenStates, int[] Dimensions);

}

