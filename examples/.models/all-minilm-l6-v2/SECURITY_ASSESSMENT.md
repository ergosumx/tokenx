# Security Assessment: all-minilm-l6-v2 (QDQ)

**Assessment Date:** 2025-10-12 23:47:20

## Overall Status

⚠️ **CAUTION** (Risk Level: MEDIUM)

> Model contains unknown operators or minor anomalies. Review carefully before deployment.

## Integrity Checks

- **Valid ONNX Syntax:** ✅ Yes
- **Passes ONNX Checker:** ✅ Yes
- **File Size:** 21.91 MB
- **SHA256:** `afdb6f1a0e45b715...`

## ONNX Runtime Compatibility

✅ **Compatible with ONNX Runtime**

- **Inputs:** 3
- **Outputs:** 1
- **Providers:** CPUExecutionProvider

## Operator Analysis

- **Total Operators:** 772
- **Unique Operators:** 22

✅ **No Suspicious Operators**

**Unknown Operators:** 8

- `Erf` (count: 6)
- `ReduceMean` (count: 26)
- `Pow` (count: 13)
- `Shape` (count: 49)
- `Constant` (count: 146)
- `DequantizeLinear` (count: 3)
- `DynamicQuantizeLinear` (count: 24)
- `MatMulInteger` (count: 36)

## Weight Analysis

- **Initializers:** 180
- **Total Parameters:** 5,120

✅ **No Weight Anomalies Detected**

---

*This assessment was automatically generated. Review carefully before production use.*
