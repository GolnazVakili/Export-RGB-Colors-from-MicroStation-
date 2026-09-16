# Export ColorIndex and RGB from MicroStation

MicroStation CONNECT / 2023+ **add-in** that exports **ColorIndex and RGB on every row**. After install, it **loads automatically** and stays on the **Drawing → Automation** tab when you close and reopen MicroStation.

## Install (once)

1. Build:

```powershell
msbuild src\ExportRgbColors\ExportRgbColors.csproj /p:Configuration=Release
```

2. Copy these files (or run `install\Install-ExportRgbColors.ps1`):

| File | Destination |
| --- | --- |
| `ExportRgbColors.dll` | `MicroStation\mdlapps\` |
| `ribbon\ExportRgbColorsRibbon.xml` | `MicroStation\mdlapps\` |
| `ribbon\ExportRgbColorsNamedCommands.xml` | `MicroStation\mdlapps\` |
| `config\ExportRgbColors.cfg` | `MicroStation\config\appl\` |

Example folders:

- `C:\Program Files\Bentley\MicroStation 2024\MicroStation\mdlapps\`
- `C:\Program Files\Bentley\MicroStation 2024\MicroStation\config\appl\`

3. **Quit MicroStation completely**, start it again, and open a DGN.

## After restart

1. Set the workflow to **Drawing**.
2. Open the **Automation** tab.
3. Click **Export RGB Colors**.

The add-in is loaded with the DGN (`MS_DGNAPPS`). The ribbon file is loaded at startup (`MS_RIBBONXML`), so the button is still there after you close and reopen MicroStation.

If the button is missing, right-click the ribbon → **Customize Ribbon** → Drawing → **Automation** → add **Export RGB Colors** from **Commands (Custom)**. The key-in is:

```text
[RgbCsvExport]RGBCSV DIALOG
```

## CSV

```text
ColorIndex,RGB,R,G,B,Layer
```

```csv
ColorIndex,RGB,R,G,B,Layer
0,"0, 0, 0",0,0,0,
1,"0, 0, 255",0,0,255,
2,"0, 255, 0",0,255,0,
```

Every color-table index `0`–`255` is written, including colors not assigned to a layer.

## Key-ins

| Key-in | Action |
| --- | --- |
| `RGBCSV DIALOG` | Add-in form (ColorIndex + RGB), then export |
| `RGBCSV EXPORT` | Same form |
| `RGBCSV EXPORT C:\temp\file-colors.csv` | Write CSV directly |
| `RGBCSV TABLE` | All 256 color codes with ColorIndex + RGB |

## Project layout

```text
src/ExportRgbColors/           C# add-in
ribbon/                        Automation-tab ribbon + named command
config/ExportRgbColors.cfg     auto-load + ribbon path
install/Install-ExportRgbColors.ps1
tests/ExportRgbColors.Tests/
```
