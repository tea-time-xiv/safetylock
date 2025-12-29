# Child Safety Lock

A Dalamud plugin that provides a soft "Child Safety Lock" feature for Final Fantasy XIV. When enabled, it blocks certain in-game interactions to prevent accidental actions.

## Features

When Child Safety Lock is enabled, the following interactions are blocked:

- **Vendor Windows** - Prevents opening vendor/shop interfaces
- **Duty Finder** - Prevents interacting with duty finder UI
- **Quest Acceptance** - Prevents accepting new quests

Each feature can be individually toggled on or off in the configuration.

## Usage

### Commands

- `/childlock` - Toggle Child Safety Lock on/off
- `/childlockspam` - Requires 5 executions within 5 seconds to toggle (prevents accidental toggles)
- `/pmycommand` - Opens the main plugin window

### Configuration

Open the configuration window via:
- The plugin installer interface
- In-game `/xlplugins` menu

Configuration options:
- **Enable Child Safety Lock** - Master on/off toggle
- **Enable on Startup** - Automatically enable Child Safety Lock when the plugin loads
- **Block Vendors** - Enable/disable vendor blocking
- **Block Duty Finder** - Enable/disable duty finder blocking
- **Block Quests** - Enable/disable quest acceptance blocking

## Important Notes

⚠️ **This is UI-level blocking only.** The plugin operates by closing or preventing UI elements from being interacted with. It does not modify game memory, intercept packets, or perform engine-level blocking. The lock is best-effort and may not catch every edge case.

## Installation

Install via the Dalamud plugin installer in XIVLauncher.

## Prerequisites

* XIVLauncher, FINAL FANTASY XIV, and Dalamud must be installed
* The game must have been run with Dalamud at least once

## Development

### Building

1. Open up `SamplePlugin.sln` in your C# editor of choice (Visual Studio 2022 or JetBrains Rider).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release`.
3. The resulting plugin can be found at `SamplePlugin/bin/x64/Debug/SamplePlugin.dll` (or `Release` if appropriate.)

### Testing in-game

1. Launch the game and use `/xlsettings` in chat to open up the Dalamud settings.
2. Go to `Experimental`, and add the full path to the `SamplePlugin.dll` to the list of Dev Plugin Locations.
3. Use `/xlplugins` to open the Plugin Installer, go to `Dev Tools > Installed Dev Plugins`, and enable the plugin.
4. You should now be able to use `/childlock` to toggle the Child Safety Lock!

## License

This project is licensed under the AGPL-3.0-or-later license. See [LICENSE.md](LICENSE.md) for details.

## Disclaimer

This plugin is 100% AI generated.