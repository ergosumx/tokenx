# TikToken Submodule Local Adjustments

This repository vendors the TikToken Rust crate under `.ext/tiktoken`. The
following local changes are applied so that the native bridge compiles cleanly
and exposes the APIs required by the .NET interop layer:

- `src/lib.rs`
  - Gate the `std::borrow::Borrow` import behind the `python` feature to avoid
    unused-import warnings when the crate is built as a pure Rust library.
  - Remove the unused `std::borrow::Cow` import.
  - Change `CoreBPE::decode_bytes` to be `pub` so the FFI layer can decode
    byte sequences without duplicating internal logic.

When updating the submodule to a newer TikToken release, re-apply the above
changes or confirm they are still present upstream.
