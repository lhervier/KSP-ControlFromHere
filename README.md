# KSP-ControlFromHere

A Kerbal Space Program mod that adds a flight-scene window listing the **command modules** of the active vessel, with one-click access to each module's part action window and to its "Control From Here" action.

## Features

- Lists every part of the active vessel that carries a `ModuleCommand` (docking ports, which expose "Control From Here" through `ModuleDockingNode`, are intentionally excluded).
- Per row:
  - the vessel-type icon (native KSP sprite),
  - the vessel name carried by the module,
  - the part title (already localized by KSP),
  - the naming priority (shown only when greater than 0).
- A **Piloting** badge marks the module that currently controls the vessel.
- Two actions per row:
  - **Show the part action window** of the part.
  - **Control from here** — shortcut for the part's "Control From Here" action. Disabled on the module that is already in control.
- The list refreshes automatically on vessel change, vessel modification (docking/undocking, part add/remove) and control-point switch.

The window is reachable from an application-launcher button (white joystick icon).

See [mockup.html](mockup.html) for the reference layout and [SPEC.md](SPEC.md) for the full specification and the design decisions behind the mod.

## Installation

You do not need to build anything: a ready-to-use archive is published with each release.

1. Download `ControlFromHereMod.zip` from the [latest release](https://github.com/lhervier/KSP-ControlFromHere/releases/latest).
2. Extract it into your KSP `GameData` folder. You should end up with:

   ```
   <KSP>/GameData/ControlFromHereMod/
   ```

   where `<KSP>` is your Kerbal Space Program installation directory (the folder containing `KSP_x64.exe`). The archive already contains the `ControlFromHereMod` folder, so just unzip it directly into `GameData`.
3. Start the game. The application-launcher button appears in the flight scene.

To update, delete the existing `GameData/ControlFromHereMod` folder and repeat the steps above. To uninstall, simply remove that folder.

> The sections below ([Building](#building), build-time [Installing](#installing)) are only needed if you want to compile the mod from sources.

## Localization

UI strings are translated through KSP's localization system. Provided languages:

- English — [GameData/ControlFromHereMod/Localization/en-us.cfg](GameData/ControlFromHereMod/Localization/en-us.cfg)
- French — [GameData/ControlFromHereMod/Localization/fr-fr.cfg](GameData/ControlFromHereMod/Localization/fr-fr.cfg)

Part titles are not translated by the mod: they come already localized from KSP (`part.partInfo.title`), which also covers parts from other mods.

## Repository layout

- `Src/` — mod sources (`com.github.lhervier.ksp.*`).
- `GameData/ControlFromHereMod/` — icon and localization files shipped with the mod.
- `KSP-Shared/` — git **submodule** ([KSP-Shared](https://github.com/lhervier/KSP-Shared.git)) providing the shared uGUI components, styles and sprite helpers. It is compiled directly from sources into the mod, producing a **single DLL**.

## Building

Building requires the KSP DLLs. Clone with submodules, set the `KSPDIR` environment variable to your KSP installation, then run the build script for your OS.

```bash
git clone --recurse-submodules https://github.com/lhervier/KSP-ControlFromHere.git
```

(If you already cloned without `--recurse-submodules`, run `git submodule update --init`.)

### Windows

```bat
set "KSPDIR=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program"
build.bat
```

### Linux

```bash
export KSPDIR="$HOME/.steam/steam/steamapps/common/Kerbal Space Program"
./build.sh
```

The build compiles the mod (together with the `KSP-Shared` sources) and produces `Release/ControlFromHereMod.zip`, packaging the DLL, the icon, the shared textures and the localization files.

## Installing (from a local build)

If you built the mod yourself, `install.bat` / `install.sh` unzips the freshly built release into your KSP `GameData` folder (using the same `KSPDIR` environment variable). End users should instead follow [Installation](#installation) above.

### Windows

```bat
install.bat
```

### Linux

```bash
./install.sh
```

You can also install manually by unzipping `Release/ControlFromHereMod.zip` into `<KSP>/GameData/`.

## Updating the shared submodule

To pull the latest `KSP-Shared` commits onto its tracked branch:

```bash
./update-submodule.sh   # or update-submodule.bat on Windows
```

Then commit the updated submodule pointer:

```bash
git add KSP-Shared && git commit -m "Bump KSP-Shared"
```

## Requirements

- Kerbal Space Program 1.12.
- .NET Framework 4.7.2 SDK / `dotnet` CLI for building.
