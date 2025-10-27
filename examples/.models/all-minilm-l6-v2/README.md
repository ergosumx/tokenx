# all-minilm-l6-v2 - QDQ

**Generated:** 2025-10-12 23:47:20

## Source

- **Repository:** [Xenova/all-MiniLM-L6-v2](https://huggingface.co/Xenova/all-MiniLM-L6-v2)
- **Precision:** QDQ
- **Download Date:** 2025-10-12

## Quick Start

```python
import onnxruntime as ort
import numpy as np

# Load model
session = ort.InferenceSession("model_quantized.onnx")

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

- `model_quantized.onnx` (21.91 MB)

### Metadata Files

- `README.md` (1.73 KB)
- `config.json` (0.63 KB)
- `special_tokens_map.json` (0.12 KB)
- `tokenizer.json` (694.98 KB)
- `tokenizer_config.json` (0.36 KB)
- `vocab.txt` (226.08 KB)

## Model Information

### Inputs

- **input_ids**: int64 ['batch_size', 'sequence_length']
- **attention_mask**: int64 ['batch_size', 'sequence_length']
- **token_type_ids**: int64 ['batch_size', 'sequence_length']

### Outputs

- **last_hidden_state**: float32 ['batch_size', 'sequence_length', 384]

### Statistics

- **Total Operations:** 772
- **Parameters:** 22,607,366
- **Model Size:** 21.71 MB

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
