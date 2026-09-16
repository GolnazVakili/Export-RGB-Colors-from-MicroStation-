# Copies Export RGB Colors into the MicroStation folder on THIS Windows PC.
# Double-click Copy-To-MicroStation.bat (Run as administrator if Program Files is locked).

param(
    [string]$MicroStationDir
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Find-MicroStation {
    if ($MicroStationDir -and (Test-Path (Join-Path $MicroStationDir "ustation.dll"))) {
        return (Resolve-Path $MicroStationDir).Path
    }
    $hints = @(
        $env:MS,
        $env:MSMDIR,
        $env:MICROSTATION_LOCATION,
        "C:\Program Files\Bentley\MicroStation 2025\MicroStation",
        "C:\Program Files\Bentley\MicroStation 2024\MicroStation",
        "C:\Program Files\Bentley\MicroStation 2023\MicroStation",
        "C:\Program Files\Bentley\MicroStation CONNECT Edition\MicroStation"
    )
    foreach ($candidate in $hints) {
        if ($candidate -and (Test-Path (Join-Path $candidate "ustation.dll"))) {
            return $candidate
        }
    }
    $bentley = "C:\Program Files\Bentley"
    if (Test-Path $bentley) {
        $found = Get-ChildItem -Path $bentley -Filter ustation.dll -Recurse -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($found) {
            return $found.DirectoryName
        }
    }
    throw "MicroStation was not found. Pass -MicroStationDir `"C:\Program Files\Bentley\<version>\MicroStation`"."
}

function Copy-File([string]$from, [string]$to) {
    $dir = Split-Path $to
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Copy-Item -Force $from $to
    Write-Host "Copied $to"
}

$ms = Find-MicroStation
$mdlapps = Join-Path $ms "mdlapps"
$appl = Join-Path $ms "config\appl"

$dll = @(
    (Join-Path $root "src\ExportRgbColors\bin\Release\ExportRgbColors.dll"),
    (Join-Path $root "src\ExportRgbColors\bin\Debug\ExportRgbColors.dll")
) | Where-Object { Test-Path $_ } | Select-Object -First 1

try {
    if ($dll) {
        Copy-File $dll (Join-Path $mdlapps "ExportRgbColors.dll")
    }
    Copy-File (Join-Path $root "ribbon\ExportRgbColorsRibbon.xml") (Join-Path $mdlapps "ExportRgbColorsRibbon.xml")
    Copy-File (Join-Path $root "ribbon\ExportRgbColorsNamedCommands.xml") (Join-Path $mdlapps "ExportRgbColorsNamedCommands.xml")
    Copy-File (Join-Path $root "config\ExportRgbColors.cfg") (Join-Path $appl "ExportRgbColors.cfg")
}
catch [System.UnauthorizedAccessException] {
    Write-Host "Program Files is locked. Re-run Copy-To-MicroStation.bat as Administrator."
    throw
}

if (-not $dll) {
    Write-Host ""
    Write-Host "Ribbon files are in place. ExportRgbColors.dll was not in bin\Release yet."
    Write-Host "On this PC run:"
    Write-Host "  msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release"
    Write-Host "then run this installer again so the DLL is copied."
}

Write-Host ""
Write-Host "Installed into $ms"
Write-Host "Quit MicroStation completely, start it, open a DGN."
Write-Host "Drawing ribbon: click the new Automation tab, then Export RGB Colors."
