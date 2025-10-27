# Security Assessment: e5-small-v2 (QDQ)

**Assessment Date:** 2025-10-12 23:34:17

## Overall Status

⚠️ **CAUTION** (Risk Level: MEDIUM)

> Model contains unknown operators or minor anomalies. Review carefully before deployment.

## Integrity Checks

- **Valid ONNX Syntax:** ✅ Yes
- **Passes ONNX Checker:** ✅ Yes
- **File Size:** 32.44 MB
- **SHA256:** `7d9092cb25f2bd1c...`

## ONNX Runtime Compatibility

✅ **Compatible with ONNX Runtime**

- **Inputs:** 3
- **Outputs:** 1
- **Providers:** CPUExecutionProvider

## Operator Analysis

- **Total Operators:** 1510
- **Unique Operators:** 22

✅ **No Suspicious Operators**

**Unknown Operators:** 8

- `Erf` (count: 12)
- `MatMulInteger` (count: 72)
- `DequantizeLinear` (count: 3)
- `Shape` (count: 97)
- `Constant` (count: 284)
- `DynamicQuantizeLinear` (count: 48)
- `Pow` (count: 25)
- `ReduceMean` (count: 50)

## Weight Analysis

- **Initializers:** 348
- **Total Parameters:** 5,120

✅ **No Weight Anomalies Detected**

---

*This assessment was automatically generated. Review carefully before production use.*
