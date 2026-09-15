# Export RGB Colors from MicroStation

MicroStation CONNECT / 2023+ C# add-in that translates the active DGN **color table** (and ByLevel / level colors) to RGB in a CSV file.

CSV columns on **every** row:

```text
ColorIndex,R,G,B,Layer
```

| Column | Color table rows | ByLevel / level rows |
| --- | --- | --- |
| `ColorIndex` | `0`–`255` (required) | **`-1`** (the Index dialog ByLevel value; required) |
| `R`, `G`, `B` | RGB for that table index | RGB of that level’s ByLevel color |
| `Layer` | blank | Level name (only when a level exists) |

There is **no** color-table row for −1. Color −1 in the CONNECT Color / Index dialog is ByLevel; that RGB is written on a **level row** with `ColorIndex,-1` and the layer name.

If several levels share one color, the file has **one row per level** (`ColorIndex` −1, same RGB, different `Layer`).

Example:

```csv
ColorIndex,R,G,B,Layer
0,0,0,0,
1,0,0,255,
-1,0,0,255,EQPM
-1,0,0,255,R-LITE
```

## Requirements

- MicroStation CONNECT Edition, 2023, 2024, or 2025 (64-bit)
- Visual Studio 2019+ or the .NET Framework 4.8 targeting pack (to build)
- The add-in is `Bentley.MstnPlatformNET` / `Bentley.DgnPlatformNET` (not V8i)

The project **auto-detects** the MicroStation folder that contains `ustation.dll` from common install paths, or from `MS` / `MSMDIR` / `MICROSTATION_LOCATION`. Override if needed:

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release /p:MicroStationDir="C:\Program Files\Bentley\MicroStation 2024\MicroStation"
```

Use the folder for **the MicroStation version you have installed** (the directory that contains `ustation.dll`).

## Build

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release
```

Output: `src\ExportRgbColors\bin\Release\ExportRgbColors.dll`

CSV formatting can be tested without MicroStation:

```powershell
dotnet test tests\ExportRgbColors.Tests\ExportRgbColors.Tests.csproj
```

## Install and load

1. Copy `ExportRgbColors.dll` into your MicroStation `mdlapps` folder, for example:
   - `C:\Program Files\Bentley\MicroStation 2024\MicroStation\mdlapps\`
   - `C:\Program Files\Bentley\MicroStation CONNECT Edition\MicroStation\mdlapps\`
2. Start MicroStation and open a DGN (for example `R-LITE-EQPM`).
3. Key-in:

```text
mdl load ExportRgbColors
```

If you keep the DLL somewhere else, append that folder to `MS_ADDINPATH` (a sample cfg is in `config/ExportRgbColors.cfg`) and restart MicroStation, then run the same `mdl load` key-in.

## Key-ins and UI

| Key-in | Action |
| --- | --- |
| `RGBCSV EXPORT` | Save dialog, then export color table + ByLevel rows |
| `RGBCSV EXPORT C:\temp\R-LITE-EQPM-colors.csv` | Export to that path (no dialog) |
| `RGBCSV DIALOG` | Small form: path, include color table, include levels |

The Message Center reports how many color-table and level rows were written.

## What is exported

1. **Color table** — 256 rows, `ColorIndex` `0`–`255`, RGB from `DgnColorMap.ExtractElementColorInfo`, `Layer` empty.
2. **ByLevel** — one row per named level. `ColorIndex` is `-1`, RGB is that level’s ByLevel color (`LevelHandle.GetByLevelColor`), `Layer` is the level name.

UTF-8 with BOM so Excel opens the file correctly. Level names that contain commas are quoted.

## Project layout

```text
src/ExportRgbColors/     C# add-in (net48, x64)
tests/ExportRgbColors.Tests/   CSV unit tests (no MicroStation)
config/ExportRgbColors.cfg     optional MS_ADDINPATH helper
```
