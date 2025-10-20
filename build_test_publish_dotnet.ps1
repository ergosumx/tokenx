[CmdletBinding()]
param(
    [switch]$SkipUnitTests,
    [switch]$SkipIntegrationTests,
    [switch]$SkipDownloads,
    [switch]$SkipBenchmarks
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

    function Resolve-PythonInterpreter {
        param(
            [string]$Root
        )

        $venvPython = Join-Path $Root '.venv/Scripts/python.exe'
        if (Test-Path $venvPython) {
            return $venvPython
        }

        $venvUnixPython = Join-Path $Root '.venv/bin/python'
        if (Test-Path $venvUnixPython) {
            return $venvUnixPython
        }

        return 'python'
    }

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = if ([string]::IsNullOrWhiteSpace($scriptRoot)) { Get-Location } else { Resolve-Path (Join-Path $scriptRoot '.') }

Write-Host "Repository root: $repoRoot"

$project = Join-Path $repoRoot 'src/ErgoX.VecraX.ML.NLP.Tokenizers/ErgoX.VecraX.ML.NLP.Tokenizers.csproj'
$testProject = Join-Path $repoRoot 'tests/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests/ErgoX.VecraX.ML.NLP.Tokenizers.HuggingFace.Tests.csproj'
$downloadScript = Join-Path $repoRoot 'tests/Py/Huggingface/download_tests_data.py'
$benchmarkScript = Join-Path $repoRoot 'tests/Py/Huggingface/generate_benchmarks.py'
$python = Resolve-PythonInterpreter $repoRoot

Write-Host "Using python interpreter: $python"
Write-Host '===> Building ErgoX.VecraX.ML.NLP.Tokenizers project'
dotnet build $project

if (-not $SkipUnitTests.IsPresent) {
    Write-Host '===> Running unit tests (Category=Unit)'
    dotnet test $testProject --filter "Category=Unit" --no-build
}
else {
    Write-Host 'Skipping unit tests'
}

if (-not $SkipDownloads.IsPresent) {
    Write-Host '===> Downloading integration test data'
    & $python $downloadScript
} else {
    Write-Host 'Skipping test data download'
}

if (-not $SkipBenchmarks.IsPresent) {
    Write-Host '===> Generating benchmark fixtures'
    & $python $benchmarkScript
} else {
    Write-Host 'Skipping benchmark generation'
}

if (-not $SkipIntegrationTests.IsPresent) {
    Write-Host '===> Running integration tests (Category=Integration)'
    dotnet test $testProject --filter "Category=Integration" --no-build
} else {
    Write-Host 'Skipping integration tests'
}

Write-Host 'Dotnet build/test workflow complete.'
