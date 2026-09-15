# Export RGB Colors from MicroStation

MicroStation CONNECT / 2023+ C# add-in that writes the active DGN **color table** and **level (layer) ByLevel colors** to a CSV file.

CSV columns:

```text
ColorIndex,Layer,R,G,B
```

| Column | Color table rows | Level / layer rows |
| --- | --- | --- |
| `ColorIndex` | `0`–`255` | ByLevel color index used by that level |
| `Layer` | blank | Level name |
| `R`, `G`, `B` | RGB 0–255 | RGB of that level’s ByLevel color |

Example:

```csv
ColorIndex,Layer,R,G,B
0,,0,0,0
1,,0,0,255
4,EQPM,0,0,255
```

Color **−1** in the CONNECT Color / Index dialog is **ByLevel**. That is not a table slot. The RGB you see for ByLevel is the active (or named) level’s color, and it is exported on a **level row** with `Layer` filled in.

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
| `RGBCSV EXPORT` | Save dialog, then export color table + level colors |
| `RGBCSV EXPORT C:\temp\R-LITE-EQPM-colors.csv` | Export to that path (no dialog) |
| `RGBCSV DIALOG` | Small form: path, include color table, include levels |

The Message Center reports how many color-table and level rows were written.

## What is exported

1. **Color table** — 256 rows, indices `0`–`255`, `Layer` empty. RGB comes from `DgnColorMap.ExtractElementColorInfo` for the active file.
2. **Levels** — one row per valid level in the file’s level cache. `Layer` is the level name; `ColorIndex` and RGB are that level’s ByLevel color (`LevelHandle.GetByLevelColor`).

UTF-8 with BOM so Excel opens the file correctly. Level names that contain commas are quoted.

## Project layout

```text
src/ExportRgbColors/     C# add-in (net48, x64)
tests/ExportRgbColors.Tests/   CSV unit tests (no MicroStation)
config/ExportRgbColors.cfg     optional MS_ADDINPATH helper
```
