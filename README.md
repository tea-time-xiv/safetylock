# Safety Lock

A Dalamud plugin that provides a soft "Safety Lock" feature for Final Fantasy XIV. When enabled, it blocks certain in-game interactions to prevent accidental actions.

## Features

When Safety Lock is enabled, the following interactions are blocked:

- **Vendor Windows** - Prevents opening vendor/shop interfaces
- **Duty Finder** - Prevents interacting with duty finder UI
- **Quest Acceptance** - Prevents accepting new quests
- **Glamour Dresser** - Prevents opening the glamour dresser
- **Armoire** - Prevents opening the armoire
- **Free Company Chest** - Prevents opening the FC chest
- **Housing Food** - Prevents consuming housing food
- **Levequests** - Prevents accepting levequests
- **Housing Retainers** - Prevents interacting with housing estate retainers
- **Additional Chambers** - Prevents entering additional chambers in FC houses
- **Housing Picture Frames** - Prevents interacting with housing picture frames

Each feature can be individually toggled on or off in the configuration.

## Usage

### Commands

- `/safetylock` - Toggle Safety Lock on/off
- `/safetylockspam` - Requires 5 executions within 5 seconds to toggle (prevents accidental toggles)

### Configuration

Open the configuration window via the plugin installer interface or the in-game `/xlplugins` menu.

Configuration options:
- **Enable Safety Lock** - Master on/off toggle
- **Enable on Startup** - Automatically enable Safety Lock when the plugin loads
- Individual block toggles for each feature

## Important Notes

⚠️ **This is UI-level blocking only.** The plugin operates by closing UI elements after they open. It does not modify game memory, intercept packets, or perform engine-level blocking. The lock is best-effort and may not catch every edge case.

## Installation

1. Add the custom repo to Dalamud: `https://raw.githubusercontent.com/tea-time-xiv/safetylock/master/pluginmaster.json`
2. Install via the Dalamud plugin installer in XIVLauncher.

## Prerequisites

* XIVLauncher, FINAL FANTASY XIV, and Dalamud must be installed

## Development

### Building

```bash
dotnet build --configuration Release
```

Output: `SafetyLockPlugin/bin/x64/Release/`

### Testing in-game

1. Use `/xlsettings` → Experimental, add the full path to `SafetyLockPlugin.dll` as a Dev Plugin Location.
2. Use `/xlplugins` → Dev Tools → Installed Dev Plugins, enable the plugin.
3. Use `/safetylock` to toggle Safety Lock.

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md) for details.

## Disclaimer

This plugin is 100% AI generated.
