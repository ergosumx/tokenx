## Remediated

- Addressed CodeQL `rust/access-invalid-pointer` by converting the raw tokenizer handle to a safe reference with `Pointer::as_ref()` guards before use, preventing unchecked dereferences in `.ext/hf_bridge/src/ffi/decode.rs`.
- `.github/workflows/hf-release.yml` now uses DigiCert's standard timestamp service (`http://timestamp.digicert.com`) with an inline suppression for DevSkim `DS137138`, documenting that DigiCert does not provide an HTTPS endpoint.

## Outstanding DevSkim Notes

- Informational: DevSkim notes for `unsafe` contexts in managed interop (`src/.../Interop/*.cs`, `src/.../Core/Tokenizer.cs`) remain as documentation reminders. No actionable remediation identified; the `unsafe` blocks are required for native interop.