﻿using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.NativeWrapper;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for vendor UI addons and reports when they appear while Child Safety Lock is enabled.
/// </summary>
public sealed class VendorWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public VendorWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "Shop", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ShopExchangeItem", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ShopExchangeCurrency", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "GoldSaucerExchange", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and vendor blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockVendors)
        {
            return;
        }

        // Close the vendor addon to block interaction
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print("Vendor interaction blocked (Child Safety Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Shop", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ShopExchangeItem", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ShopExchangeCurrency", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "GoldSaucerExchange", OnAddonPostSetup);
    }
}

