[CmdletBinding()]
param(
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = if ([string]::IsNullOrWhiteSpace($scriptRoot)) { Get-Location } else { Resolve-Path (Join-Path $scriptRoot '.') }

Write-Host "Repository root: $repoRoot"

$rustBridge = Join-Path $repoRoot '.ext/hf_bridge'
$manifest = Join-Path $rustBridge 'Cargo.toml'
$targetRoot = Join-Path $rustBridge 'target'
$targetDir = Join-Path $targetRoot 'release'
$runtimeRoot = Join-Path $repoRoot 'src/ErgoX.VecraX.ML.NLP.Tokenizers/HuggingFace/runtimes'

if (-not (Test-Path $targetDir)) {
    $candidateReleases = Get-ChildItem -Path $targetRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $releasePath = Join-Path $_.FullName 'release'
        if (Test-Path $releasePath) {
            [PSCustomObject]@{
                Path = $releasePath
                LastWriteTime = (Get-Item $releasePath).LastWriteTimeUtc
            }
        }
    } | Sort-Object -Property LastWriteTime -Descending

    if ($candidateReleases) {
        $targetDir = $candidateReleases[0].Path
    } else {
        throw "Unable to locate release artifacts under $targetRoot after build."
    }
}

Write-Host '===> Building Rust bridge (release configuration)'
cargo build --manifest-path $manifest --release

if (-not $SkipTests.IsPresent) {
    Write-Host '===> Running Rust tests'
    cargo test --manifest-path $manifest
}

$processArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
$osRuntime = if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    switch ($processArchitecture) {
        ([System.Runtime.InteropServices.Architecture]::X86) { 'win-x86' }
        ([System.Runtime.InteropServices.Architecture]::Arm64) { 'win-arm64' }
        Default { 'win-x64' }
    }
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
    if ($processArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
        'linux-arm64'
    } else {
        'linux-x64'
    }
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    if ($processArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
        'osx-arm64'
    } else {
        'osx-x64'
    }
} else {
    throw 'Unsupported operating system for runtime publish.'
}

$destination = Join-Path $runtimeRoot "$osRuntime/native"
if (-not (Test-Path $destination)) {
    Write-Host "Creating runtime directory: $destination"
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}

$artifacts = @()
if ($osRuntime -like 'win-*') {
    $artifacts = @('tokenx_bridge.dll', 'tokenx_bridge.pdb')
} elseif ($osRuntime -like 'linux-*' -or $osRuntime -like 'android-*') {
    $artifacts = @('libtokenx_bridge.so')
} elseif ($osRuntime -like 'osx-*') {
    $artifacts = @('libtokenx_bridge.dylib')
} elseif ($osRuntime -like 'ios-*') {
    $artifacts = @('libtokenx_bridge.a', 'libtokenx_bridge.dylib') | Where-Object {
        Test-Path (Join-Path $targetDir $_)
    }
    if (-not $artifacts) {
        throw 'No iOS bridge artifacts were produced.'
    }
} else {
    throw "Unsupported runtime identifier mapping: $osRuntime"
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
