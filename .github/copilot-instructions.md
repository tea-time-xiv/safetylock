# Child Lock - Development Instructions

## Project Goal

Child Lock is a Dalamud plugin that provides a soft "child lock" feature for Final Fantasy XIV. When enabled, it blocks certain in-game interactions to prevent accidental actions:

- Vendor windows
- Duty Finder interactions
- Quest acceptance dialogs

**Important:** This is UI-level blocking only. We do not modify game memory, intercept packets, or perform engine-level blocking. The lock is best-effort and operates by closing or preventing UI elements from being interacted with.

## Design Principles

- **Small, incremental steps** – Make one change at a time and verify it works.
- **One feature at a time** – Don't start on quest blocking until vendor blocking is complete.
- **Prefer simple, boring solutions first** – If a straightforward approach works, use it.
- **No premature abstraction** – Don't create frameworks or base classes "for the future."
- **No overengineering** – Solve the problem at hand, not imagined future problems.

## Architectural Rules

- **Global plugin state must live in a config object** – The config is the source of truth for what's enabled.
- **All blocking decisions must be driven by config flags** – If `config.BlockVendors` is false, vendors aren't blocked.
- **UI / addon hooks must be thin** – Hook callbacks should delegate to simple, testable logic classes.
- **Avoid static state** – Pass dependencies through constructors; make the plugin easy to reason about.
- **Each blocker lives in its own class** – `VendorBlocker`, `DutyFinderBlocker`, etc. Each is responsible for one type of interaction.

## Coding Guidelines

- **Follow existing Dalamud sample plugin patterns** – Stay consistent with the SamplePlugin structure.
- **Use constructor injection for Dalamud services** – Request `IClientState`, `IAddonLifecycle`, etc. via constructor parameters with `[PluginService]` attributes.
- **Always unsubscribe from hooks/events in Dispose()** – Prevent memory leaks and crashes on plugin unload.
- **Prefer readable code over clever code** – Future maintainers (including you) will thank you.
- **No reflection, memory patching, or packet manipulation** – Use only supported Dalamud APIs.

## Safety Constraints

- **Only use supported Dalamud APIs** – If it's not in the official API surface, don't use it.
- **Only close or cancel UI addons** – Never write to game memory or modify data structures.
- **Accept that blocking is "best effort"** – It's a soft lock, not a hard lock. Edge cases may exist.

## Scope Control

Start with these features only:

1. **Global on/off toggle** in the config UI
2. **Vendor blocking** – Close vendor windows when the lock is enabled

**Future features** (duty finder blocking, quest acceptance blocking, etc.) are added **only after** the current feature is complete, tested, and working.

Do not implement features speculatively. Do not add hooks or classes for features not yet in scope.

---

*This document guides all development decisions. When in doubt, refer back to these principles.*

### Dalamud addon pointer handling
- Never cast args.Addon or NativePointer manually.
- Always use Dalamud.Game.NativeWrapper.AtkUnitBasePtr.
- Use pattern matching and call addon.Value->Close(true).
- Do not use FFXIVClientStructs for addon UI pointers.
