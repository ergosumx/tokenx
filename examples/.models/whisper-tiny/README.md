# whisper-tiny - QDQ

**Generated:** 2025-10-12 21:11:30

## Source

- **Repository:** [Xenova/whisper-tiny](https://huggingface.co/Xenova/whisper-tiny)
- **Precision:** QDQ
- **Download Date:** 2025-10-12

## Quick Start

```python
import onnxruntime as ort
import numpy as np

# Load model
session = ort.InferenceSession("decoder_model_merged_quantized.onnx")

# Get input/output info
inputs = session.get_inputs()
outputs = session.get_outputs()

print(f"Model inputs: {[inp.name for inp in inputs]}")
print(f"Model outputs: {[out.name for out in outputs]}")

# Run inference (example)
# input_data = np.random.randn(1, 3, 224, 224).astype(np.float32)
# results = session.run(None, {inputs[0].name: input_data})
```

## Files

### ONNX Models

- `decoder_model_merged_quantized.onnx` (29.30 MB)
- `decoder_model_quantized.onnx` (29.05 MB)
- `decoder_with_past_model_quantized.onnx` (27.88 MB)
- `encoder_model_quantized.onnx` (9.66 MB)

### Metadata Files

- `README.md` (1.11 KB)
- `added_tokens.json` (2.03 KB)
- `config.json` (2.20 KB)
- `generation_config.json` (3.63 KB)
- `merges.txt` (482.29 KB)
- `preprocessor_config.json` (0.33 KB)
- `quant_config.json` (2.77 KB)
- `quantize_config.json` (2.77 KB)
- `special_tokens_map.json` (2.14 KB)
- `tokenizer.json` (2422.33 KB)
- `tokenizer_config.json` (276.06 KB)
- `vocab.json` (1012.29 KB)

## Model Information

### Inputs

- **input_ids**: int64 ['batch_size', 'decoder_sequence_length']
- **encoder_hidden_states**: float32 ['batch_size', 'encoder_sequence_length / 2', 384]
- **past_key_values.0.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.0.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.0.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.0.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.1.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.1.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.1.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.1.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.2.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.2.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.2.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.2.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.3.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.3.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length', 64]
- **past_key_values.3.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **past_key_values.3.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **use_cache_branch**: bool [1]

### Outputs

- **logits**: float32 ['batch_size', 'decoder_sequence_length', 51865]
- **present.0.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.0.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.0.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.0.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.1.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.1.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.1.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.1.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.2.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.2.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.2.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.2.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.3.decoder.key**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.3.decoder.value**: float32 ['batch_size', 6, 'past_decoder_sequence_length + 1', 64]
- **present.3.encoder.key**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **present.3.encoder.value**: float32 ['batch_size', 6, 'encoder_sequence_length_out', 64]
- **decoder_attentions.0**: float32 ['batch_size', 6, 'decoder_sequence_length', 'past_decoder_sequence_length + 1']
- **decoder_attentions.1**: float32 ['batch_size', 6, 'decoder_sequence_length', 'past_decoder_sequence_length + 1']
- **decoder_attentions.2**: float32 ['batch_size', 6, 'decoder_sequence_length', 'past_decoder_sequence_length + 1']
- **decoder_attentions.3**: float32 ['batch_size', 6, 'decoder_sequence_length', 'past_decoder_sequence_length + 1']
- **cross_attentions.0**: float32 ['batch_size', 6, 'decoder_sequence_length', 'encoder_sequence_length_out']
- **cross_attentions.1**: float32 ['batch_size', 6, 'decoder_sequence_length', 'encoder_sequence_length_out']
- **cross_attentions.2**: float32 ['batch_size', 6, 'decoder_sequence_length', 'encoder_sequence_length_out']
- **cross_attentions.3**: float32 ['batch_size', 6, 'decoder_sequence_length', 'encoder_sequence_length_out']

### Statistics

- **Total Operations:** 2
- **Parameters:** 29,552,338
- **Model Size:** 28.75 MB

## Validation Status

✅ **ONNX Runtime Compatible**

- **Providers:** CUDAExecutionProvider, CPUExecutionProvider

## Usage Notes


### QDQ (Quantize-Dequantize)
- **Description:** Quantization-aware model with Q/DQ operations
- **Use Case:** INT8 quantization simulation for accuracy testing
- **Size:** Similar to FP32 (contains quantization metadata)
- **Speed:** Can be converted to INT8 for faster inference
- **Accuracy:** Close to FP32 accuracy

- This model was automatically downloaded and validated
- See `SECURITY_ASSESSMENT.md` for security analysis
- See `ARCHITECTURE.md` for detailed architecture
- See `VALIDATION_REPORT.json` for complete validation results

---

*Auto-generated documentation*
