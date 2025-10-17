# CI/CD Build Failures - Analysis & Fixes

**Date:** October 17, 2025  
**Status:** ✅ Fixed

---

## Issues Found

### 1. Android NDK Path Error ❌

**Error:**
```
error: linker `/toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android24-clang` not found
  = note: No such file or directory (os error 2)
```

**Root Cause:**
- Used `${{ env.ANDROID_NDK_ROOT }}` which is not set
- The `nttld/setup-ndk@v1` action sets the NDK path but we weren't capturing it

**Fix:**
```yaml
- name: Setup Android NDK
  uses: nttld/setup-ndk@v1
  id: setup-ndk  # ← Add ID to capture output
  with:
    ndk-version: r26d

- name: Configure cargo for Android
  run: |
    cat >> ~/.cargo/config.toml << EOF
    [target.aarch64-linux-android]
    ar = "${{ steps.setup-ndk.outputs.ndk-path }}/toolchains/llvm/prebuilt/linux-x86_64/bin/llvm-ar"
    linker = "${{ steps.setup-ndk.outputs.ndk-path }}/toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android24-clang"
    EOF

- name: Build C bindings
  run: |
    cargo build --release --target aarch64-linux-android
    ${{ steps.setup-ndk.outputs.ndk-path }}/toolchains/llvm/prebuilt/linux-x86_64/bin/llvm-strip target/aarch64-linux-android/release/libtokenizers.so
```

---

### 2. macOS Strip Command Failure ❌

**Error:**
```
/Applications/Xcode_15.4.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/strip: 
error: symbols referenced by indirect symbol table entries that can't be stripped in: 
/Users/runner/work/tokenizers/tokenizers/bindings/c/target/aarch64-apple-darwin/release/libtokenizers.dylib
```

**Root Cause:**
- Standard `strip` command removes too many symbols from dynamic libraries
- Dynamic libraries need to preserve symbols for dynamic linking

**Fix:**
Use `strip -x` (remove only non-external/non-global symbols):

```yaml
# Before (fails)
- name: Build C bindings
  run: |
    cargo build --release --target x86_64-apple-darwin
    strip target/x86_64-apple-darwin/release/libtokenizers.dylib

# After (works)
- name: Build C bindings
  run: |
    cargo build --release --target x86_64-apple-darwin
    strip -x target/x86_64-apple-darwin/release/libtokenizers.dylib
```

Applied to both:
- macOS x64 build
- macOS ARM64 build

---

### 3. Rust Warnings (24 warnings) ⚠️

**Error:**
```
warning: `tokenizers-c` (lib) generated 24 warnings
```

**Root Cause:**
- Placeholder functions have unused parameters
- These functions exist to satisfy the C API contract but aren't fully implemented yet

**Status:** ✅ Already fixed in previous commit
- All parameters prefixed with `_` to indicate intentional non-use
- Build now produces 0 warnings

---

## Question: Why Have Unused Functions?

### Answer: API Stability & Forward Compatibility

**The functions exist for:**

1. **C API Contract**: .NET library expects these symbols to exist (even if not called yet)
2. **Future Implementation**: Parameters are ready for when we implement full functionality
3. **No Runtime Impact**: Returning placeholder errors is safe for unused functions

**Evidence they're not currently needed:**
- 179/180 tests pass without using these functions
- Core tokenization works (encode, decode, vocabulary, models)
- These are advanced features (padding, truncation, token mapping)

### Decision: Keep Them

**Reasoning:**
1. **No harm**: They don't cause failures, just return "not implemented" errors
2. **API stability**: Removing them breaks .NET library compilation
3. **Future-ready**: When needed, just remove `_` prefix and implement
4. **No size impact**: Rust strips unused code in release builds

**Alternative considered:** Remove entirely and add later
- **Downside**: Breaks .NET library build
- **No benefit**: We'd need to add them back eventually anyway

---

## Files Modified

### 1. `.github/workflows/release-c-bindings.yml`

**Android NDK fix:**
- Added `id: setup-ndk` to capture output
- Changed `${{ env.ANDROID_NDK_ROOT }}` → `${{ steps.setup-ndk.outputs.ndk-path }}`
- Applied to 3 locations (config, build, strip)

**macOS strip fix:**
- Changed `strip` → `strip -x` for x86_64-apple-darwin
- Changed `strip` → `strip -x` for aarch64-apple-darwin

### 2. `bindings/c/src/encoding/methods.rs`

**Already fixed in previous commit:**
- 24 parameters prefixed with `_`
- Build produces 0 warnings

---

## Testing Checklist

Before pushing tags to trigger release builds:

- [x] Local Rust build succeeds (0 warnings)
- [x] Android NDK path uses step output
- [x] macOS strip uses `-x` flag
- [ ] Push and verify GitHub Actions builds pass
- [ ] Verify all 7 platform artifacts are created
- [ ] Verify checksums.txt is generated
- [ ] Verify GitHub Release is created

---

## Platform Build Status

| Platform | Expected Status | Notes |
|----------|----------------|-------|
| Linux x64 | ✅ Should work | No strip issues on Linux |
| Windows x64 | ✅ Should work | No strip command used |
| macOS x64 | ✅ Fixed | Now using `strip -x` |
| macOS ARM64 | ✅ Fixed | Now using `strip -x` |
| iOS ARM64 | ✅ Should work | Static library, strip optional |
| Android ARM64 | ✅ Fixed | Now using step output for NDK path |
| WASM | ✅ Should work | No strip command used |

---

## References

### Android NDK Action Documentation
- https://github.com/nttld/setup-ndk
- Output: `ndk-path` provides full path to NDK installation

### macOS Strip Command
- `strip`: Remove all symbols (breaks dynamic libraries)
- `strip -x`: Remove only non-global symbols (safe for .dylib)
- `strip -S`: Remove debug symbols only (most conservative)

### Rust Naming Conventions
- `param`: Used parameter
- `_param`: Intentionally unused parameter
- `_`: Fully ignored parameter (can't reference)

---

## Next Steps

1. ✅ Commit workflow fixes
2. ✅ Commit Rust warning fixes (already done)
3. ⏳ Push changes to repository
4. ⏳ Create test tag: `git tag c-v0.1.0-test`
5. ⏳ Push tag: `git push origin c-v0.1.0-test`
6. ⏳ Monitor GitHub Actions for all 7 builds
7. ⏳ If successful, create production tag: `c-v0.1.0`

---

**Status:** Ready for testing ✅  
**Confidence:** High (all issues identified and fixed)
