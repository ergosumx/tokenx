# CI/CD Workflows

This document describes the automated testing and release workflows for the ErgoX VecraX Tokenizers project.

## Overview

The project uses GitHub Actions for continuous integration and deployment with three main workflows:

1. **Rust C Bindings Tests** - Tests the Rust FFI layer
2. **.NET Integration Tests** - Tests the complete .NET → Rust integration
3. **Release with Test Reports** - Builds multi-platform binaries and publishes test results

## Workflows

### 1. Rust C Bindings Test & Coverage

**File**: `.github/workflows/test-c-bindings.yml`

**Triggers**:
- Push to `main`, `master`, or `develop` branches (when C bindings change)
- Pull requests to main branches
- Manual dispatch

**Jobs**:
- **test**: Runs Rust tests on Linux, Windows, and macOS
- **coverage**: Generates code coverage reports using cargo-tarpaulin
- **test-summary**: Aggregates results and provides status

**Outputs**:
- ✅ Test results for 16 Rust decoder tests
- 📊 Code coverage reports (HTML, XML, Lcov)
- 🔗 Codecov integration
- 💬 PR comments with coverage summary

**Badge**: 
```markdown
![Rust Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml/badge.svg)
```

---

### 2. .NET Integration Tests

**File**: `.github/workflows/test-dotnet.yml`

**Triggers**:
- Push to main branches (when .NET code or Rust bindings change)
- Pull requests
- Manual dispatch

**Jobs**:
- **build-rust**: Builds Rust library for Linux, Windows, macOS
- **test-dotnet**: Runs 180 .NET integration tests on all platforms
- **test-summary**: Generates comprehensive test reports

**Test Execution**:
```bash
dotnet test --configuration Release \
  --logger "trx;LogFileName=test-results.trx" \
  --logger "html;LogFileName=test-results.html" \
  --collect:"XPlat Code Coverage"
```

**Outputs**:
- ✅ Test results (180 tests: 179 passed, 1 skipped)
- 📊 Coverage reports for .NET code
- 📁 Test artifacts (TRX, HTML, OpenCover XML)
- 💬 PR comments with test matrix

**Test Retention**: 
- Test results: 30 days
- Combined reports: 90 days

**Badge**:
```markdown
![.NET Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml/badge.svg)
```

---

### 3. Release with Test Reports

**File**: `.ext/tokenizers/.github/workflows/release-c-bindings.yml`

**Triggers**:
- Tags matching `c-v*.*.*` (e.g., `c-v0.22.1`)
- Manual dispatch

**Build Targets**:
1. **Linux x64** - `x86_64-unknown-linux-gnu`
2. **Windows x64** - `x86_64-pc-windows-msvc`
3. **macOS x64** - `x86_64-apple-darwin`
4. **macOS ARM64** - `aarch64-apple-darwin`
5. **iOS ARM64** - `aarch64-apple-ios`
6. **Android ARM64** - `aarch64-linux-android`
7. **WebAssembly** - `wasm32-unknown-unknown`

**Release Process**:
1. ✅ Build binaries for all platforms
2. ✅ Run .NET integration tests on Linux, Windows, macOS
3. ✅ Package test reports (HTML + TRX)
4. ✅ Generate SHA-256 checksums
5. ✅ Create GitHub Release with:
   - Binary artifacts (`.tar.gz`, `.zip`)
   - Test reports archive (`test-reports.tar.gz`)
   - Checksums file
   - Release notes with test results

**Release Assets**:
```
tokenizers-c-linux-x64.tar.gz
tokenizers-c-win-x64.zip
tokenizers-c-osx-x64.tar.gz
tokenizers-c-osx-arm64.tar.gz
tokenizers-c-ios-arm64.tar.gz
tokenizers-c-android-arm64.tar.gz
tokenizers-c-wasm.tar.gz
test-reports.tar.gz          ← Test results
checksums.txt
```

**Release Notes Format**:
```markdown
## HuggingFace Tokenizers C Bindings v0.22.1

### ✅ Test Results
All 180 .NET integration tests passed on Linux, Windows, and macOS platforms.
- **Total Tests**: 180
- **Passed**: 179
- **Skipped**: 1 (known Rust library limitation)
- **Failed**: 0

📊 **Download `test-reports.tar.gz` for detailed test results**

### Supported Platforms
[... platform list ...]

### Installation
[... installation instructions ...]
```

---

## Test Reports

### Report Formats

**TRX (Visual Studio Test Results)**:
- Machine-readable XML format
- Integrates with Azure DevOps, Visual Studio
- Contains detailed test execution data

**HTML Reports**:
- Human-readable test results
- Includes test names, durations, outcomes
- Viewable in any web browser

**OpenCover XML**:
- Code coverage data
- Compatible with Codecov, Coveralls
- Shows line/branch coverage percentages

### Accessing Test Reports

**During Development**:
1. Go to Actions tab on GitHub
2. Select workflow run
3. Download artifacts:
   - `test-results-ubuntu-latest`
   - `test-results-windows-latest`
   - `test-results-macos-latest`
   - `combined-test-results`

**From Releases**:
1. Go to Releases page
2. Download `test-reports.tar.gz`
3. Extract and open HTML files in browser

**Coverage Reports**:
- Codecov: https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers
- Download HTML from workflow artifacts

---

## Running Tests Locally

### Rust Tests

```bash
cd .ext/tokenizers/bindings/c
cargo test --release
```

### .NET Tests

```bash
# Build Rust library first
cd .ext/tokenizers/bindings/c
cargo build --release

# Copy DLL to runtime folder
$srcPath = ".ext/tokenizers/bindings/c/target/release/tokenizers.dll"
$destDir = "src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native"
New-Item -ItemType Directory -Force -Path $destDir
Copy-Item $srcPath $destDir -Force

# Run tests
dotnet test --configuration Release
```

### With Coverage

```bash
# Rust coverage
cd .ext/tokenizers/bindings/c
cargo install cargo-tarpaulin
cargo tarpaulin --out Html --output-dir coverage

# .NET coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

---

## CI/CD Maintenance

### Adding New Tests

1. Add tests to appropriate test project
2. Commit and push
3. Workflow automatically runs tests
4. Check workflow results in Actions tab

### Updating Workflows

**Rust Bindings Workflow**:
- Edit `.github/workflows/test-c-bindings.yml`
- Modify test job steps if needed
- Update coverage configuration in `Cargo.toml`

**.NET Tests Workflow**:
- Edit `.github/workflows/test-dotnet.yml`
- Adjust build matrix for new platforms
- Update test commands or reporters

**Release Workflow**:
- Edit `.ext/tokenizers/.github/workflows/release-c-bindings.yml`
- Add new build targets in matrix
- Update release notes template

### Creating a Release

1. **Tag the release**:
   ```bash
   git tag c-v0.22.2
   git push origin c-v0.22.2
   ```

2. **Workflow automatically**:
   - Builds all platforms
   - Runs integration tests
   - Packages test reports
   - Creates GitHub Release

3. **Verify release**:
   - Check Actions tab for workflow status
   - Review release notes
   - Download and verify test reports

---

## Troubleshooting

### Test Failures

**Rust Tests Failing**:
- Check Rust toolchain version
- Verify Cargo.lock is committed
- Review test output in Actions logs

**.NET Tests Failing**:
- Ensure Rust library is built
- Check DLL is in correct runtime folder
- Verify .NET SDK version (8.0.x)
- Review P/Invoke signatures match Rust exports

### Workflow Failures

**Build Job Failing**:
- Check disk space on runner
- Verify cargo cache is valid
- Review compiler errors in logs

**Coverage Job Failing**:
- Ensure cargo-tarpaulin is compatible
- Check if tests are running successfully first
- Verify coverage configuration in Cargo.toml

**Release Job Failing**:
- Ensure all build jobs succeeded
- Check artifact downloads completed
- Verify GITHUB_TOKEN has permissions

### Performance Issues

**Slow Builds**:
- Verify caching is working correctly
- Check if incremental compilation is enabled
- Consider splitting large test suites

**Long Test Runs**:
- Use `--release` mode for faster execution
- Consider parallelizing test execution
- Review test timeouts

---

## Monitoring & Notifications

### GitHub Actions

- All workflows visible in **Actions** tab
- Email notifications for failed runs
- Branch protection rules can require status checks

### Codecov Integration

- Automatic coverage reporting
- PR comments with coverage diff
- Coverage badges for README

### Status Badges

Add these to your README.md:

```markdown
[![Rust Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-c-bindings.yml)

[![.NET Tests](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml/badge.svg)](https://github.com/ergosumx/vecrax-hf-tokenizers/actions/workflows/test-dotnet.yml)

[![codecov](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers/branch/main/graph/badge.svg)](https://codecov.io/gh/ergosumx/vecrax-hf-tokenizers)
```

---

## Best Practices

### For Contributors

1. ✅ Run tests locally before pushing
2. ✅ Ensure all tests pass in CI
3. ✅ Review coverage reports for new code
4. ✅ Update tests when adding features
5. ✅ Check workflow status before merging PRs

### For Maintainers

1. ✅ Review test results in PRs
2. ✅ Monitor test trends over time
3. ✅ Keep workflows up to date
4. ✅ Rotate secrets regularly
5. ✅ Archive old test artifacts

### For Releases

1. ✅ Verify all tests pass before tagging
2. ✅ Review test reports in release artifacts
3. ✅ Update changelog with test statistics
4. ✅ Document any known test limitations
5. ✅ Communicate test results to users

---

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [cargo-tarpaulin](https://github.com/xd009642/tarpaulin)
- [dotnet test Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [Codecov Documentation](https://docs.codecov.com/)
- [Test Reporter Action](https://github.com/dorny/test-reporter)

---

**Last Updated**: October 17, 2025  
**Maintained by**: ErgoX VecraX Team
