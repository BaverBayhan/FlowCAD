# FlowCAD

FlowCAD is an AutoCAD automation toolkit for designing sewer and drainage systems. It combines a custom AutoCAD .NET plugin with a Windows Forms desktop controller to guide engineers through the full sewer-network design workflow — from placing manholes to computing pipe slopes and exporting results.

## Repository Structure

```
FlowCAD/
├── FlowCAD-core/          # AutoCAD .NET plugin (loaded via NETLOAD)
├── FlowCAD-desktop/       # Windows Forms GUI controller
├── BoyProfilUIAutomation/ # Longitudinal-profile (boy profil) UI automation
├── ExcelAutomation/       # Excel export automation (in development)
└── acad-libraries/        # AutoCAD .NET API reference assemblies
```

## Components

### FlowCAD-core
A .NET class library that registers custom AutoCAD commands. It is loaded into a running AutoCAD session using `NETLOAD`. The plugin implements a strict state-machine that enforces the correct design sequence.

### FlowCAD-desktop
A Windows Forms application that acts as a command panel for FlowCAD-core. It connects to a running AutoCAD instance via COM, loads the plugin, and exposes buttons that send the appropriate AutoCAD commands.

### BoyProfilUIAutomation
A standalone executable that automates the boy profil (longitudinal/vertical profile) dialog inside AutoCAD using mouse-click simulation.

## Workflow & Modes

FlowCAD enforces a linear design pipeline through seven modes. Each mode unlocks a specific set of commands and can only be entered when the previous stage is complete.

| Mode | Turkish Name | Description |
|------|-------------|-------------|
| **MB** | Muayene Bacası | Place manholes on the drawing |
| **KKY** | Kapak Kotu Yükseltme | Attach a detail circle (*izahat çemberi*) to every manhole |
| **FMKNA** | Fener Muayene Kota Nokta Atama | Assign cover elevation (*kapak kotu*) to each manhole |
| **AKARKOT** | Akar Kot | Assign invert elevation (*akar kotu*) to each manhole |
| **DRAW** | — | Draw aligned dimensions between manholes |
| **CALCULATE** | — | Compute pipe slopes, depths, and hydraulic parameters |
| **REVERT** | — | Roll back drawing objects to a previous state |

Valid mode transitions:

```
MB → KKY → FMKNA → AKARKOT → DRAW → CALCULATE → REVERT
 ↑_____________________________|___________|___________|
```

> All modes allow a direct return to **MB** mode. **REVERT** also allows jumping back to **CALCULATE** or **DRAW**.

## Prerequisites

- **AutoCAD** (any version that supports the .NET API, e.g., AutoCAD 2020+)
- **.NET Framework 4.8**
- **Windows** (AutoCAD COM automation is Windows-only)

## Getting Started

### Build

Open the desired solution in Visual Studio and build in Release configuration:

| Solution | Output |
|----------|--------|
| `FlowCAD-core/FlowCAD-core.sln` | `FlowCAD-core.dll` |
| `FlowCAD-desktop/FlowCAD-desktop.sln` | `FlowCAD-desktop.exe` |
| `BoyProfilUIAutomation/BoyProfilUIAutomation.sln` | `BoyProfilUIAutomation.exe` |

Place the three output files in the same directory before running.

### Running

1. Open AutoCAD and load a drawing file.
2. Launch **FlowCAD-desktop.exe**.
3. Click **Connect** — the application will attach to the running AutoCAD instance.
4. Click **Start Automation** to load `FlowCAD-core.dll` into AutoCAD via `NETLOAD`.
5. Use the mode selector to advance through the design stages and press the corresponding command buttons.

### Design Sequence

1. **MB mode** → Click *Create Muayene Bacası* repeatedly to place all manholes.
2. **KKY mode** → For each manhole the view zooms in; click to place its detail circle.
3. **FMKNA mode** → Assign a cover elevation value to every detail circle.
4. **AKARKOT mode** → Assign an invert elevation value to every manhole.
5. **DRAW mode** → Click *Draw Dimensions* to annotate pipe segments.
6. **CALCULATE mode** → Click *Calculate System* to compute hydraulic results.
7. *(Optional)* **REVERT mode** → Undo drawn objects and recalculate if needed.

## AutoCAD Commands Reference

The following commands are registered by `FlowCAD-core.dll`:

| Command | Available In | Description |
|---------|-------------|-------------|
| `CHANGEMODE` | Any | Switch to a different mode |
| `RESETMODE` | Any | Reset to MB mode |
| `CREATEMUAYENEBACASI` | MB | Place a new manhole circle |
| `CREATEIZAHATCEMBERI` | KKY | Attach a detail circle to each manhole |
| `ASSIGNKAPAKKOT` | FMKNA | Assign cover elevation |
| `ASSIGNAKARKOT` | AKARKOT | Assign invert elevation |
| `DRAWDIMENSIONS` | DRAW | Draw aligned dimensions |
| `CALCULATESYSTEM` | CALCULATE | Run hydraulic calculations |
| `REVERT` | REVERT | Remove generated objects |
| `DEFINEMAINHAT` | — | Mark the main pipe line |
| `MANUELDIMENSION` | — | Manually add a dimension |
| `BOYPROFIL` | — | Launch the longitudinal profile |

## Project Notes

- All drawing metadata (elevations, IDs, linked object handles) is stored in AutoCAD **Xrecord** objects keyed by the entity handle in the Named Objects Dictionary.
- Manholes are identified by a `class = muayene_bacası` attribute in their Xrecord.
- Pipe lines (*hat*) are grouped by a `Mb_hat_id` GUID so that calculations iterate each line independently.
- The minimum manhole depth constant is **1.65 m**; the maximum slope denominator for 200 mm pipe is **200**.
