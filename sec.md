## Remediated

- Addressed CodeQL `rust/access-invalid-pointer` by converting the raw tokenizer handle to a safe reference with `Pointer::as_ref()` guards before use, preventing unchecked dereferences in `.ext/hf_bridge/src/ffi/decode.rs`.
- Removed NuGet package signing from `.github/workflows/hf-release.yml`, eliminating the previous DevSkim `DS137138` timestamp finding entirely.
- Hardened `.github/workflows/hf-rc.yml` GitHub Packages publishing step to require an explicit API key (PAT fallback and extended timeout) so authentication and upload failures are surfaced early.

## Outstanding DevSkim Notes

- Informational: DevSkim notes for `unsafe` contexts in managed interop (`src/.../Interop/*.cs`, `src/.../Core/Tokenizer.cs`) remain as documentation reminders. No actionable remediation identified; the `unsafe` blocks are required for native interop.