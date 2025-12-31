using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for Glamour Dresser UI addons and blocks them when Child Safety Lock is enabled.
/// </summary>
public sealed class GlamourDresserWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public GlamourDresserWatcher(IAddonLifecycle addonLifecycle, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for Glamour Dresser addons
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "MiragePrismPrismBox", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "MiragePrismBox", OnAddonPostSetup);
    }

    private unsafe void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and glamour dresser blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockGlamourDresser)
        {
            return;
        }

        // Close the glamour dresser addon to block interaction
        var addonPtr = args.Addon;
        var addon = (AtkUnitBase*)(nint)addonPtr;
        if (addon == null)
        {
            return;
        }

        addon->Close(true);
        chatGui.Print("Glamour Dresser blocked (Child Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "MiragePrismPrismBox", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "MiragePrismBox", OnAddonPostSetup);
    }
}

