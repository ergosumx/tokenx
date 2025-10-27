# Security Assessment: whisper-tiny (QDQ)

**Assessment Date:** 2025-10-12 21:11:30

## Overall Status

❌ **WARNING** (Risk Level: HIGH)

> Model has suspicious operators or integrity issues. Do not use in production without thorough review.

## Integrity Checks

- **Valid ONNX Syntax:** ✅ Yes
- **Passes ONNX Checker:** ✅ Yes
- **File Size:** 29.3 MB
- **SHA256:** `6c0c125986b007d2...`

## ONNX Runtime Compatibility

✅ **Compatible with ONNX Runtime**

- **Inputs:** 19
- **Outputs:** 25
- **Providers:** CPUExecutionProvider

## Operator Analysis

- **Total Operators:** 2
- **Unique Operators:** 2

⚠️ **Suspicious Operators Found:** 1

- `If` (count: 1)

**Unknown Operators:** 1

- `DynamicQuantizeLinear` (count: 1)

## Weight Analysis

- **Initializers:** 182
- **Total Parameters:** 175,488

✅ **No Weight Anomalies Detected**

---

*This assessment was automatically generated. Review carefully before production use.*
