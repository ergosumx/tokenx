# Getting On NuGet

This document describes everything required to promote a HuggingFace release candidate to a signed NuGet release by using the `hf-release` workflow.

## Release Flow Overview
- Run the `hf-rc` workflow to produce the desired release candidate (RC) packages. Make note of the generated RC semantic version (for example, `0.22.1-rc.8`).
- Verify the RC locally (for example, via `examples/tools/NugetTests`) and ensure all automated checks are green.
- Trigger the `hf-release` workflow manually:
  1. Provide the RC version (e.g. `0.22.1-rc.8`).
  2. Select the corresponding HuggingFace bridge build (defaults to the latest successful run).
  3. The workflow rebuilds packages without the `-rc.x` suffix, signs them, publishes them to GitHub Packages and NuGet.org, and creates a `hf-v<version>` GitHub release.

## Required GitHub Secrets
Add the following repository secrets before running `hf-release`:
- `TOKENX_SIGNING_CERT_PFX`: Base64-encoded contents of `.certs/tokenx-signing.pfx` (contains the private key).
- `TOKENX_SIGNING_CERT_PASSWORD`: Password that protects the `.pfx` file.
- `TOKENX_NUGET_API_KEY`: API key for NuGet.org publishing (create at https://www.nuget.org/account/apikeys with "Push" scope).

### Encoding the Certificate
Use PowerShell to create the base64 payload expected by the workflow:
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes(".certs/tokenx-signing.pfx"))
```
Copy the printed string into the `TOKENX_SIGNING_CERT_PFX` secret. Do **not** add extra whitespace or line breaks.

### Secret Management Notes
- Keep the `.pfx` password in a password manager; only reference it through the `TOKENX_SIGNING_CERT_PASSWORD` secret.
- Rotate the NuGet API key periodically and update the `TOKENX_NUGET_API_KEY` secret accordingly.
- The public `.cer` file may be stored with source control for reference, but it is not required by the workflows.

## Running `hf-release`
1. Navigate to _Actions → HuggingFace Release_ and choose **Run workflow**.
2. Fill in the **Release candidate version** (e.g. `0.22.1-rc.8`).
3. Leave **Bridge run selection** as `latest-success`, or choose a specific run if you need to pin to an older bridge build (supply the matching run id/number when prompted).
4. Click **Run workflow**.
5. Monitor the job stages:
   - Runtime staging (pulls bridge artifacts for every RID).
   - Build + packaging (generates final `.nupkg` and runtime-only packages).
   - Tests against the newly built release package.
   - Package signing (uses `TOKENX_SIGNING_CERT_*` secrets).
   - Publication to GitHub Packages and NuGet.org.
   - GitHub release creation (`hf-v<version>` tag plus assets).
6. Download the workflow artifact `hf-release-packages-<version>` if you need a local copy of the signed packages.

## Operational Checklist
- ✅ Ensure `hf-rc` for the targeted version completed successfully.
- ✅ Confirm the release candidate passes local validation (e.g. `dotnet run` inside `examples/tools/NugetTests`).
- ✅ Verify the three secrets above exist and are up to date.
- ✅ Trigger `hf-release` and confirm the run finishes without warnings.
- ✅ Validate the packages on NuGet.org (listing + download) once the workflow completes.

Following this checklist keeps the final publication path deterministic and auditable.
