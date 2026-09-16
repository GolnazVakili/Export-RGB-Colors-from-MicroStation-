# Install ExportRgbColors into a local MicroStation CONNECT / 2023+ folder.
# Run from the repo (or from the folder that contains src\ and ribbon\):
#   powershell -ExecutionPolicy Bypass -File install\Install-ExportRgbColors.ps1

param(
    [string]$MicroStationDir
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Find-MicroStation {
    if ($MicroStationDir -and (Test-Path (Join-Path $MicroStationDir "ustation.dll"))) {
        return $MicroStationDir
    }
    foreach ($candidate in @(
            $env:MS,
            $env:MSMDIR,
            $env:MICROSTATION_LOCATION,
            "C:\Program Files\Bentley\MicroStation 2025\MicroStation",
            "C:\Program Files\Bentley\MicroStation 2024\MicroStation",
            "C:\Program Files\Bentley\MicroStation 2023\MicroStation",
            "C:\Program Files\Bentley\MicroStation CONNECT Edition\MicroStation"
        )) {
        if ($candidate -and (Test-Path (Join-Path $candidate "ustation.dll"))) {
            return $candidate
        }
    }
    throw "MicroStation was not found. Pass -MicroStationDir `"C:\Program Files\Bentley\<version>\MicroStation`"."
}

$ms = Find-MicroStation
$mdlapps = Join-Path $ms "mdlapps"
$appl = Join-Path $ms "config\appl"
New-Item -ItemType Directory -Force -Path $mdlapps, $appl | Out-Null

$dllCandidates = @(
    (Join-Path $root "src\ExportRgbColors\bin\Release\ExportRgbColors.dll"),
    (Join-Path $root "src\ExportRgbColors\bin\Debug\ExportRgbColors.dll")
)
$dll = $dllCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $dll) {
    throw "ExportRgbColors.dll not found. Build the add-in first (msbuild ... /p:Configuration=Release)."
}

Copy-Item -Force $dll (Join-Path $mdlapps "ExportRgbColors.dll")
Copy-Item -Force (Join-Path $root "ribbon\ExportRgbColorsRibbon.xml") (Join-Path $mdlapps "ExportRgbColorsRibbon.xml")
Copy-Item -Force (Join-Path $root "ribbon\ExportRgbColorsNamedCommands.xml") (Join-Path $mdlapps "ExportRgbColorsNamedCommands.xml")
Copy-Item -Force (Join-Path $root "config\ExportRgbColors.cfg") (Join-Path $appl "ExportRgbColors.cfg")

Write-Host "Installed to $ms"
Write-Host "Copied:"
Write-Host "  $mdlapps\ExportRgbColors.dll"
Write-Host "  $mdlapps\ExportRgbColorsRibbon.xml"
Write-Host "  $mdlapps\ExportRgbColorsNamedCommands.xml"
Write-Host "  $appl\ExportRgbColors.cfg"
Write-Host ""
Write-Host "Close MicroStation completely, start it again, open a DGN."
Write-Host "On the Drawing ribbon click the new Automation tab, then Export RGB Colors."
