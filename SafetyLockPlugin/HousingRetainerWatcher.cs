using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for housing retainer UI addons and blocks access while Child Safety Lock is enabled.
/// </summary>
public sealed class HousingRetainerWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public HousingRetainerWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for housing retainer-related addons
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerList", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerSellList", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerSell", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and housing retainer blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockHousingRetainers)
        {
            return;
        }

        // Close the housing retainer addon to block interaction
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print("Housing retainer interaction blocked (Child Safety Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerList", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSellList", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSell", OnAddonPostSetup);
    }
}

