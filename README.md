# Export ColorIndex and RGB from MicroStation

MicroStation CONNECT / 2023+ **add-in** that exports **ColorIndex and RGB on every row at the same time** — the same pair shown in the Color / Index dialog.

Every attached color-table code (`0`–`255`) is written, including colors that are not assigned to a layer.

## CSV

```text
ColorIndex,RGB,R,G,B,Layer
```

| Column | Color-table rows | Optional ByLevel rows |
| --- | --- | --- |
| `ColorIndex` | `0`–`255` | `-1` |
| `RGB` | `R, G, B` as in the Color dialog | same for that level |
| `R`, `G`, `B` | numeric components | numeric components |
| `Layer` | blank | level name |

Example:

```csv
ColorIndex,RGB,R,G,B,Layer
0,"0, 0, 0",0,0,0,
1,"0, 0, 255",0,0,255,
2,"0, 255, 0",0,255,0,
```

## Install the add-in

1. Build (on a machine with MicroStation):

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release
```

If MicroStation is not in a default folder:

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release /p:MicroStationDir="C:\Program Files\Bentley\MicroStation 2024\MicroStation"
```

2. Copy `src\ExportRgbColors\bin\Release\ExportRgbColors.dll` to `MicroStation\mdlapps\`, for example:
   - `C:\Program Files\Bentley\MicroStation 2024\MicroStation\mdlapps\`
   - `C:\Program Files\Bentley\MicroStation CONNECT Edition\MicroStation\mdlapps\`
3. Start MicroStation, open a DGN, and key-in:

```text
mdl load ExportRgbColors
RGBCSV DIALOG
```

The add-in form lists **ColorIndex**, a color swatch, and **RGB** together. Export writes that table to CSV.

To load automatically when a DGN opens, copy `config/ExportRgbColors.cfg` to `MicroStation\config\appl\` and uncomment `MS_DGNAPPS > ExportRgbColors`.

If the DLL is not in `mdlapps`, set `MS_ADDINPATH` in that cfg to the folder that contains it.

## Key-ins

| Key-in | Action |
| --- | --- |
| `RGBCSV DIALOG` | Add-in form: ColorIndex + RGB preview, then export |
| `RGBCSV EXPORT` | Same form (no path given) |
| `RGBCSV EXPORT C:\temp\file-colors.csv` | Write CSV directly (still ColorIndex + RGB on every row) |
| `RGBCSV TABLE` | All 256 color codes with ColorIndex + RGB |

You can also put `[RgbCsvExport]RGBCSV DIALOG` on a ribbon button so the add-in loads and opens the form.

## What is exported

1. **All color codes** — 256 rows. Each row has `ColorIndex` and `RGB` together. Unused table slots are still written.
2. **ByLevel (optional)** — extra rows with `ColorIndex` `-1`, that level’s RGB, and the layer name.

UTF-8 with BOM so Excel opens the file correctly.

CSV formatting can be tested without MicroStation:

```powershell
dotnet test tests\ExportRgbColors.Tests\ExportRgbColors.Tests.csproj
```

## Project layout

```text
src/ExportRgbColors/           C# add-in (net48, x64)
tests/ExportRgbColors.Tests/   CSV unit tests (no MicroStation)
config/ExportRgbColors.cfg     MS_ADDINPATH / optional auto-load
```
