using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for Armoire UI addons and blocks them when Child Safety Lock is enabled.
/// </summary>
public sealed class ArmoireWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public ArmoireWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for all Armoire-related addons
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "Cabinet", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "CabinetWithdraw", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "CabinetCategory", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and armoire blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockArmoire)
        {
            return;
        }

        // Close the armoire addon to block interaction
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print("Armoire blocked (Child Safety Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Cabinet", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "CabinetWithdraw", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "CabinetCategory", OnAddonPostSetup);
    }
}

