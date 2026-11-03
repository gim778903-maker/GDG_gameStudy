# Project-scoped launcher for the official Unity CLI.
# All arguments are forwarded; the project directory is selected automatically.
$ErrorActionPreference = 'Stop'
$unityCommand = Get-Command unity -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
$unityExecutable = if ($unityCommand) { $unityCommand.Source } else { Join-Path $env:LOCALAPPDATA 'Unity/bin/unity.exe' }
if (-not (Test-Path -LiteralPath $unityExecutable)) {
    throw 'Unity CLI is not installed. See Tools/UNITY-CLI.md.'
}
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    & $unityExecutable @args
    if ($LASTEXITCODE -ne 0) { throw "Unity CLI exited with code $LASTEXITCODE." }
}
finally { Pop-Location }
