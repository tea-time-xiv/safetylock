using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for Free Company Chest UI addons and blocks access while Child Safety Lock is enabled.
/// </summary>
public sealed class FreeCompanyChestWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public FreeCompanyChestWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for all FC chest-related addons
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "FreeCompanyChest", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "FreeCompanyChestItem", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "FreeCompanyChestGil", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and FC chest blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockFreeCompanyChest)
        {
            return;
        }

        // Close the FC chest addon to block interaction
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print("Free Company Chest interaction blocked (Child Safety Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "FreeCompanyChest", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "FreeCompanyChestItem", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "FreeCompanyChestGil", OnAddonPostSetup);
    }
}

