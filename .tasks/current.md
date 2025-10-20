### Todo
- [ ] Analyze Rust bridge crate structure and references for rename impact
- [x] Rename src/_hf_bridge directory from the previous Rust bridge path
- [ ] Update Rust bridge path references across build scripts, CI workflows, and solution files
- [ ] Adjust Cargo workspace or project config if needed after rename
- [ ] Design FFI surface for models/decoders bindings in Rust bridge
- [ ] Implement Rust bindings for tokenizers models (BPE, WordPiece, WordLevel, Unigram)
- [ ] Implement Rust bindings for tokenizers decoders per requirements
- [ ] Add unit tests validating new bindings
- [ ] Run formatting and unit tests for Rust bridge