# Export RGB Colors from MicroStation

Exports **every color code in the attached MicroStation color table** (indices `0`–`255`) with its **RGB**, including colors that are **not assigned to any layer**.

This is the full color table from `ActiveDesignFile.ExtractColorTable` / `GetColors`, not a list of level colors only.

CSV columns on every row:

```text
ColorIndex,R,G,B,Layer
```

| Column | Color-table rows (always 256) | Optional ByLevel rows |
| --- | --- | --- |
| `ColorIndex` | `0`–`255` | `-1` |
| `R`, `G`, `B` | RGB for that table index | RGB of that level’s ByLevel color |
| `Layer` | blank | Level name |

Example — unused codes still appear (color `2` is black here because nothing customized it):

```csv
ColorIndex,R,G,B,Layer
0,0,0,0,
1,0,0,255,
2,0,255,0,
255,128,128,128,
```

Optional extra rows (only if you ask for levels) look like:

```csv
-1,0,0,255,EQPM
-1,0,0,255,R-LITE
```

## Fastest path: VBA inside MicroStation

No compile step. Open a DGN, then:

1. Copy `vba/ExportAllColorCodes.bas` onto the machine that has MicroStation.
2. In MicroStation: **Utilities → Macros → VBA** (or **Drawing → Macros**) and import the `.bas`.
3. Key-in:

```text
vba run ExportAllColorCodes
```

That writes `<dgn>-all-color-codes.csv` next to the DGN with **all 256 color codes and RGB**. To also append ByLevel rows:

```text
vba run ExportAllColorCodesAndLevels
```

## C# add-in (CONNECT / 2023+)

### Requirements

- MicroStation CONNECT Edition, 2023, 2024, or 2025 (64-bit)
- Visual Studio 2019+ or the .NET Framework 4.8 targeting pack (to build)

The project **auto-detects** the MicroStation folder that contains `ustation.dll` from common install paths, or from `MS` / `MSMDIR` / `MICROSTATION_LOCATION`. Override if needed:

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release /p:MicroStationDir="C:\Program Files\Bentley\MicroStation 2024\MicroStation"
```

### Build

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release
```

Output: `src\ExportRgbColors\bin\Release\ExportRgbColors.dll`

CSV formatting can be tested without MicroStation:

```powershell
dotnet test tests\ExportRgbColors.Tests\ExportRgbColors.Tests.csproj
```

### Install and load

1. Copy `ExportRgbColors.dll` into your MicroStation `mdlapps` folder, for example:
   - `C:\Program Files\Bentley\MicroStation 2024\MicroStation\mdlapps\`
   - `C:\Program Files\Bentley\MicroStation CONNECT Edition\MicroStation\mdlapps\`
2. Start MicroStation and open a DGN.
3. Key-in:

```text
mdl load ExportRgbColors
```

If you keep the DLL somewhere else, append that folder to `MS_ADDINPATH` (a sample cfg is in `config/ExportRgbColors.cfg`) and restart MicroStation, then run the same `mdl load` key-in.

### Key-ins and UI

| Key-in | Action |
| --- | --- |
| `RGBCSV TABLE` | All 256 color-table codes with RGB (no layer filter) |
| `RGBCSV EXPORT` | Same: all 256 color codes with RGB |
| `RGBCSV EXPORT C:\temp\file-all-color-codes.csv` | Export to that path (no dialog) |
| `RGBCSV DIALOG` | Form: all color codes on by default; check ByLevel to also append layer rows |

The Message Center reports how many color-table codes were written.

## What is exported

1. **All color codes** — 256 rows, `ColorIndex` `0`–`255`, RGB from the **attached color table** (`ExtractColorTable` / `GetColors`). `Layer` is empty. Indices that no level uses are still written.
2. **ByLevel (optional)** — one extra row per named level. `ColorIndex` is `-1`, RGB is that level’s ByLevel color, `Layer` is the level name.

UTF-8 with BOM so Excel opens the file correctly. Level names that contain commas are quoted.

## Project layout

```text
src/ExportRgbColors/              C# add-in (net48, x64)
tests/ExportRgbColors.Tests/      CSV unit tests (no MicroStation)
vba/ExportAllColorCodes.bas       run inside MicroStation, no compile
scripts/export_microstation_color_table.py   optional COM helper (Windows)
config/ExportRgbColors.cfg        optional MS_ADDINPATH helper
```
