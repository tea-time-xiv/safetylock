using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SamplePlugin;

/// <summary>
/// Watches for quest acceptance and completion UI addons and blocks them when child lock is enabled.
/// </summary>
public sealed class QuestWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public QuestWatcher(IAddonLifecycle addonLifecycle, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for quest-related windows
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "JournalAccept", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "JournalResult", OnAddonPostSetup);
    }

    private unsafe void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if child lock is enabled
        if (!configuration.ChildLockEnabled)
        {
            return;
        }

        // Close the quest addon to block interaction
        var addonPtr = args.Addon;
        var addon = (AtkUnitBase*)(nint)addonPtr;
        if (addon == null)
        {
            return;
        }

        addon->Close(true);
        chatGui.Print("Quest interaction blocked (Child Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "JournalAccept", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "JournalResult", OnAddonPostSetup);
    }
}

