[CmdletBinding()]
param(
    [switch]$SkipDotNetTests,
    [switch]$SkipCargoTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = if ([string]::IsNullOrWhiteSpace($scriptRoot)) { Get-Location } else { Resolve-Path $scriptRoot }

$rustBridge = Join-Path $root 'src/ErgoX.VecraX.ML.NLP.Tokenizers.Rust.Bridge'
$manifest = Join-Path $rustBridge 'Cargo.toml'
$solution = Join-Path $root 'TokenX.HF.sln'
$releaseDir = Join-Path $rustBridge 'target/release'
$destination = Join-Path $root 'src/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace/runtimes/win-x64/native'
$artifacts = @('tokenx_bridge.dll', 'tokenx_bridge.pdb')

Write-Host 'Building Rust bridge (release)...'
cargo build --release --manifest-path $manifest

if (-not $SkipCargoTests.IsPresent) {
    Write-Host 'Running Rust tests...'
    cargo test --manifest-path $manifest
}

if (-not $SkipDotNetTests.IsPresent) {
    Write-Host 'Running .NET tests...'
    dotnet test $solution
}

Write-Host "Copying artifacts to $destination"
if (-not (Test-Path $destination)) {
    New-Item -Path $destination -ItemType Directory -Force | Out-Null
}

foreach ($artifact in $artifacts) {
    $sourcePath = Join-Path $releaseDir $artifact
    if (-not (Test-Path $sourcePath)) {
        throw "Expected artifact not found: $sourcePath"
    }

    Copy-Item -Path $sourcePath -Destination $destination -Force
}

Write-Host 'Done.'
