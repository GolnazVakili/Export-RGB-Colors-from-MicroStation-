# Export ColorIndex and RGB from MicroStation

MicroStation CONNECT / 2023+ **add-in** that exports **ColorIndex, RGB, Layer, and Description** on every row. After install it adds an **Automation** tab to the Drawing ribbon and puts **Export RGB Colors** on that tab.

## Install (once)

1. Build:

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release
```

2. On **your Windows PC** (the one that runs MicroStation), double-click `install\Copy-To-MicroStation.bat`. Use **Run as administrator** if it cannot write to Program Files.

That script copies the DLL, ribbon XML, and cfg into `mdlapps` and `config\appl`. This cloud agent cannot write into `C:\Program Files\Bentley\...` on your machine.

| File | Destination |
| --- | --- |
| `ExportRgbColors.dll` | `MicroStation\mdlapps\` |
| `ribbon\ExportRgbColorsRibbon.xml` | `MicroStation\mdlapps\` |
| `ribbon\ExportRgbColorsNamedCommands.xml` | `MicroStation\mdlapps\` |
| `config\ExportRgbColors.cfg` | `MicroStation\config\appl\` |

Example:

- `C:\Program Files\Bentley\MicroStation 2024\MicroStation\mdlapps\`
- `C:\Program Files\Bentley\MicroStation 2024\MicroStation\config\appl\`

3. **Quit MicroStation completely**, start it again, and open a DGN.

## Where the button is

Your Drawing tabs are currently:

File, Home, View, Annotate, Attach, Analyze, Curves, Constraints, Utilities, Drawing Aids, Content, Collaborate, Help

After install and restart, **Automation** appears in that row (next to Collaborate / Help). Click it, then **Export RGB Colors**.

Until the ribbon files are copied, you can still run it with **F9**:

```text
mdl load ExportRgbColors
RGBCSV DIALOG
```

## CSV

```csv
ColorIndex,RGB,R,G,B,Layer,Description
0,"0, 0, 0",0,0,0,,
1,"0, 0, 255",0,0,255,EQPM,Equipment
1,"0, 0, 255",0,0,255,R-LITE,Road lighting
```

Every color-table index `0`–`255` is written, including colors not assigned to a layer. **Layer** and **Description** come from each MicroStation level that uses that color (Level Manager name and description). Unused colors keep those two columns blank. If several levels share a color, that ColorIndex is repeated once per level.

## Key-ins

| Key-in | Action |
| --- | --- |
| `RGBCSV DIALOG` | Add-in form (ColorIndex, RGB, Layer, Description), then export |
| `RGBCSV EXPORT` | Same form |
| `RGBCSV EXPORT C:\temp\file-colors.csv` | Write CSV directly (includes Layer and Description) |
| `RGBCSV TABLE` | All 256 color codes with ColorIndex, RGB, Layer, Description |

## Project layout

```text
src/ExportRgbColors/           C# add-in
ribbon/                        Automation tab + named command
config/ExportRgbColors.cfg     auto-load + ribbon path
install/Install-ExportRgbColors.ps1
tests/ExportRgbColors.Tests/
```
