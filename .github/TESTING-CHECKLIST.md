# Testing & Release Checklist

Quick reference for running tests and creating releases.

## ✅ Pre-Commit Checklist

### Local Testing

- [ ] **Build Rust library**
  ```bash
  cd src/ErgoX.Vecrax.ML.NLP.Tokenizers.Rust.Bridge
  cargo build --release
  ```

- [ ] **Run Rust tests** (16 tests)
  ```bash
  cargo test --release
  ```

- [ ] **Copy DLL to .NET runtime**
  ```powershell
  $srcPath = "src/ErgoX.Vecrax.ML.NLP.Tokenizers.Rust.Bridge/target/release/tokenx_bridge.dll"
  $destDir = "src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native"
  New-Item -ItemType Directory -Force -Path $destDir
  Copy-Item $srcPath $destDir -Force
  ```

- [ ] **Run .NET tests**
  ```bash
  dotnet test --configuration Release
  ```

- [ ] **Verify test results**
  - Expected: 37 passed, 0 skipped, 0 failed

---

## 🚀 Pull Request Checklist

### Before Creating PR

- [ ] All local tests passing
- [ ] No compiler warnings or errors
- [ ] Code follows project standards (see `.github/instructions/ergox.engineering.coding.standards.instructions.md`)
- [ ] Updated relevant documentation

### After Creating PR

- [ ] **Check CI workflows** (GitHub Actions tab)
  - [ ] `test-c-bindings.yml` - Rust tests on Linux, Windows, macOS ✅
  - [ ] `test-dotnet.yml` - .NET tests on all platforms ✅

- [ ] **Review PR comments**
  - [ ] Test summary table
  - [ ] Coverage report (if enabled)
  
- [ ] **Verify all checks pass**
  - [ ] Build successful on all platforms
  - [ ] All tests passing
  - [ ] No security issues

- [ ] **Address review feedback**
  - [ ] Fix failing tests
  - [ ] Improve coverage if needed
  - [ ] Update documentation

---

## 📦 Release Checklist

### Pre-Release

- [ ] **All tests passing** in CI
  ```bash
  # Check Actions tab - ensure green builds
  ```

- [ ] **Version updated**
  - [ ] Update version in `src/ErgoX.Vecrax.ML.NLP.Tokenizers.Rust.Bridge/Cargo.toml`
  - [ ] Update CHANGELOG.md with changes

- [ ] **Documentation current**
  - [ ] README.md reflects new version
  - [ ] API changes documented
  - [ ] Migration guide (if breaking changes)

### Create Release

- [ ] **Tag release**
  ```bash
  git tag c-v0.22.2
  git push origin c-v0.22.2
  ```

- [ ] **Monitor workflow** (`release-c-bindings.yml`)
  - [ ] Build jobs (7 platforms) ✅
  - [ ] test-before-release job ✅
  - [ ] Create release job ✅

### Post-Release

- [ ] **Verify release on GitHub**
  - [ ] All 8 artifacts present:
    - [ ] `tokenx-bridge-linux-x64.tar.gz`
    - [ ] `tokenx-bridge-win-x64.zip`
    - [ ] `tokenx-bridge-osx-x64.tar.gz`
    - [ ] `tokenx-bridge-osx-arm64.tar.gz`
    - [ ] `tokenx-bridge-ios-arm64.tar.gz`
    - [ ] `tokenx-bridge-android-arm64.tar.gz`
    - [ ] `tokenx-bridge-wasm.tar.gz`
    - [ ] `test-reports.tar.gz` ⭐
    - [ ] `checksums.txt`

- [ ] **Verify release notes**
  - [ ] Test results section present
  - [ ] Shows 37/37/0/0 (total/passed/skipped/failed)
  - [ ] Platform list complete
  - [ ] Installation instructions clear

- [ ] **Download and verify test reports**
  ```bash
  # Download test-reports.tar.gz from release
  tar -xzf test-reports.tar.gz
  # Open test-results.html in browser
  ```

- [ ] **Smoke test release**
  - [ ] Download Windows x64 build
  - [ ] Verify DLL works with .NET sample project
  - [ ] Check basic tokenization functionality

- [ ] **Announce release**
  - [ ] Update project README with latest version
  - [ ] Notify users/team
  - [ ] Post to relevant channels

---

## 🔧 Troubleshooting

### Rust Tests Failing

1. **Check Rust version**
   ```bash
   rustc --version  # Should be 1.70+
   ```

2. **Clean and rebuild**
   ```bash
   cargo clean
   cargo build --release
   cargo test --release
   ```

3. **Check test output**
   ```bash
   cargo test --release -- --nocapture
   ```

### .NET Tests Failing

1. **Verify DLL location**
  ```powershell
  Test-Path "src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native/tokenx_bridge.dll"
  ```

2. **Check DLL size** (should be ~4.3 MB)
  ```powershell
  (Get-Item "src/ErgoX.Vecrax.ML.NLP.Tokenizers.Rust.Bridge/target/release/tokenx_bridge.dll").Length / 1MB
  ```

3. **Run verbose tests**
   ```bash
   dotnet test --configuration Release --verbosity detailed
   ```

4. **Check specific test**
   ```bash
   dotnet test --configuration Release --filter "FullyQualifiedName~TokenizerTests.DecodeTest"
   ```

### CI Workflow Failing

1. **Check workflow logs** in GitHub Actions tab

2. **Review common issues**:
   - Cargo cache corruption → Re-run workflow
   - Disk space issues → Clear cache
   - Network timeouts → Re-run workflow
   - Permissions → Check GITHUB_TOKEN scopes

3. **Test locally with Act** (optional)
   ```bash
   # Install Act: https://github.com/nektos/act
   act -W .github/workflows/test-dotnet.yml
   ```

### Release Workflow Issues

1. **Verify tag format** (must be `c-v*.*.*`)
   ```bash
   git tag -l "c-v*"
   ```

2. **Check build artifacts**
   - All build jobs succeeded?
   - Artifacts uploaded correctly?

3. **Test job failures**
   - Download artifact manually
   - Extract and test locally
   - Check test logs

4. **Release creation fails**
   - GITHUB_TOKEN permissions?
   - Artifact checksums match?
   - Release notes template valid?

---

## 📊 Test Statistics

### Expected Results

| Platform | Rust Tests | .NET Tests | Coverage |
|----------|-----------|-----------|----------|
| Linux    | 16/16 ✅  | 179/180 ✅ | ~75%     |
| Windows  | 16/16 ✅  | 179/180 ✅ | ~75%     |
| macOS    | 16/16 ✅  | 179/180 ✅ | ~75%     |

### Known Limitations

- **1 Skipped Test**: `TokenizerTests.ComplexPipelineTest` 
  - Reason: Rust library limitation with complex normalizers
  - Tracked in issue #123 (example)
  - Not a blocker for release

---

## 🎯 Quick Commands

### Build Everything
```bash
# Rust
cd .ext/tokenizers/bindings/c && cargo build --release

# Copy DLL (Windows)
Copy-Item src/ErgoX.Vecrax.ML.NLP.Tokenizers.Rust.Bridge/target/release/tokenx_bridge.dll src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native/ -Force

# .NET
dotnet build --configuration Release
```

### Run All Tests
```bash
# Rust
cd .ext/tokenizers/bindings/c && cargo test --release

# .NET
dotnet test --configuration Release
```

### Generate Coverage
```bash
# Rust
cd .ext/tokenizers/bindings/c
cargo tarpaulin --out Html --output-dir coverage

# .NET
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### Create Release
```bash
# Tag and push
git tag c-v0.22.2
git push origin c-v0.22.2

# Workflow automatically:
# - Builds all platforms
# - Runs tests
# - Packages reports
# - Creates release
```

---

## 📚 Resources

- **CI/CD Documentation**: `.github/CI-CD-WORKFLOWS.md`
- **Coding Standards**: `.github/instructions/ergox.engineering.coding.standards.instructions.md`
- **Acceptance Criteria**: `.github/instructions/ergox.acceptance.instructions.md`
- **Actions Tab**: https://github.com/ergosumx/vecrax-hf-tokenizers/actions

---

**Last Updated**: October 17, 2025  
**Maintained by**: ErgoX VecraX Team
