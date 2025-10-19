[CmdletBinding()]
param(
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = if ([string]::IsNullOrWhiteSpace($scriptRoot)) { Get-Location } else { Resolve-Path (Join-Path $scriptRoot '.') }

Write-Host "Repository root: $repoRoot"

$rustBridge = Join-Path $repoRoot 'src/ErgoX.VecraX.ML.NLP.Tokenizers.Rust.Bridge'
$manifest = Join-Path $rustBridge 'Cargo.toml'
$targetDir = Join-Path $rustBridge 'target/release'
$runtimeRoot = Join-Path $repoRoot 'src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes'

Write-Host '===> Building Rust bridge (release configuration)'
cargo build --manifest-path $manifest --release

if (-not $SkipTests.IsPresent) {
    Write-Host '===> Running Rust tests'
    cargo test --manifest-path $manifest
}

$osRuntime = if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    'win-x64'
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
    'linux-x64'
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    'osx-x64'
} else {
    throw 'Unsupported operating system for runtime publish.'
}

$destination = Join-Path $runtimeRoot "$osRuntime/native"
if (-not (Test-Path $destination)) {
    Write-Host "Creating runtime directory: $destination"
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}

$artifacts = @()
if ($osRuntime -eq 'win-x64') {
    $artifacts = @('tokenx_bridge.dll', 'tokenx_bridge.pdb')
} elseif ($osRuntime -eq 'linux-x64') {
    $artifacts = @('libtokenx_bridge.so')
} else {
    $artifacts = @('libtokenx_bridge.dylib')
}

Write-Host "===> Publishing artifacts to $destination"
foreach ($artifact in $artifacts) {
    $source = Join-Path $targetDir $artifact
    if (-not (Test-Path $source)) {
        throw "Expected artifact not found: $source"
    }

    Copy-Item -Path $source -Destination $destination -Force
}

Write-Host 'Rust bridge build/test/publish complete.'
