using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SamplePlugin;

/// <summary>
/// Watches for Duty Finder UI addons and blocks them when child lock is enabled.
/// </summary>
public sealed class DutyFinderWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public DutyFinderWatcher(IAddonLifecycle addonLifecycle, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinder", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinderConfirm", OnAddonPostSetup);
    }

    private unsafe void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if child lock is enabled
        if (!configuration.ChildLockEnabled)
        {
            return;
        }

        // Close the Duty Finder addon to block interaction
        var addonPtr = args.Addon;
        var addon = (AtkUnitBase*)(nint)addonPtr;
        if (addon == null)
        {
            return;
        }

        addon->Close(true);
        chatGui.Print("Duty Finder blocked (Child Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ContentsFinder", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ContentsFinderConfirm", OnAddonPostSetup);
    }
}

