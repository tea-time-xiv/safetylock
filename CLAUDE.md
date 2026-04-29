# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build

```bash
dotnet build                          # Debug build
dotnet build --configuration Release  # Release build
```

Output lands in `SafetyLockPlugin/bin/x64/Debug/` (or `Release/`). Dalamud is fetched automatically via `Dalamud.NET.Sdk`. CI runs on Windows; local Linux builds work if Dalamud NuGet packages resolve.

No test suite exists. Verification is done in-game: add the DLL as a dev plugin via `/xlsettings` → Experimental.

## Architecture

`Plugin.cs` is the entry point. It holds all `[PluginService]` injected Dalamud services as static properties and wires up every watcher and window.

`Configuration.cs` is the single source of truth for all runtime state. Every watcher reads directly from it. `Configuration.Save()` persists via `PluginInterface.SavePluginConfig`.

Each blocked interaction lives in its own `*Watcher` class (e.g. `VendorWatcher`, `DutyFinderWatcher`). All watchers follow the same pattern:
- Constructor: `IAddonLifecycle.RegisterListener(AddonEvent.PostSetup, "AddonName", handler)`
- Handler: check `config.ChildLockEnabled && config.BlockXxx`, then `gameGui.GetAddonByName(...)` and close it
- `Dispose()`: `UnregisterListener` for every registered event

Addon closing uses a direct FFXIVClientStructs cast:
```csharp
unsafe { ((AtkUnitBase*)addon.Address)->Close(true); }
```
Note: copilot-instructions.md prescribes `Dalamud.Game.NativeWrapper.AtkUnitBasePtr` instead, but existing watchers use the FFXIVClientStructs approach. Match the pattern of the file you're editing.

`HousingRetainerWatcher` is the only watcher with extra logic: it checks `ITargetManager.Target.ObjectKind` to skip summoning bells (`EventObj`) and only block housing estate retainers. This heuristic is acknowledged as imperfect in a comment in that file.

`Windows/ConfigWindow.cs` renders all config checkboxes. Feature checkboxes are `ImGui.BeginDisabled()` when the lock is off.

## Adding a new blocker

1. Add a `BlockXxx` flag to `Configuration.cs` (default `true`).
2. Create `XxxWatcher.cs` following the exact structure of `VendorWatcher.cs`.
3. Instantiate it in `Plugin.cs`, add it to `Dispose()`, and add a checkbox in `ConfigWindow.cs`.
4. Do not add anything else — no base classes, no shared helpers.

## Key constraints

- UI-level blocking only. No memory writes, no packet interception.
- Only use supported Dalamud API surface (`IAddonLifecycle`, `IGameGui`, `IChatGui`, `ITargetManager`, etc.).
- Always unsubscribe in `Dispose()`.
- `Configuration` is never modified outside user action or startup — do not persist `ChildLockEnabled` state changes from `EnableOnStartup` back to disk.
