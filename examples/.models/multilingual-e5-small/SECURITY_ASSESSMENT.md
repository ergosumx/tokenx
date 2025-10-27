# Security Assessment: multilingual-e5-small (QDQ)

**Assessment Date:** 2025-10-12 23:37:26

## Overall Status

⚠️ **CAUTION** (Risk Level: MEDIUM)

> Model contains unknown operators or minor anomalies. Review carefully before deployment.

## Integrity Checks

- **Valid ONNX Syntax:** ✅ Yes
- **Passes ONNX Checker:** ✅ Yes
- **File Size:** 112.83 MB
- **SHA256:** `f80102d3f2a1229f...`

## ONNX Runtime Compatibility

✅ **Compatible with ONNX Runtime**

- **Inputs:** 3
- **Outputs:** 1
- **Providers:** CPUExecutionProvider

## Operator Analysis

- **Total Operators:** 1511
- **Unique Operators:** 22

✅ **No Suspicious Operators**

**Unknown Operators:** 8

- `Erf` (count: 12)
- `DynamicQuantizeLinear` (count: 48)
- `Shape` (count: 97)
- `DequantizeLinear` (count: 3)
- `MatMulInteger` (count: 72)
- `Constant` (count: 285)
- `ReduceMean` (count: 50)
- `Pow` (count: 25)

## Weight Analysis

- **Initializers:** 347
- **Total Parameters:** 4,992

✅ **No Weight Anomalies Detected**

---

*This assessment was automatically generated. Review carefully before production use.*
