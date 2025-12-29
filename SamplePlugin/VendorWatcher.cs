using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SamplePlugin;

/// <summary>
/// Watches for vendor UI addons and reports when they appear while child lock is enabled.
/// </summary>
public sealed class VendorWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public VendorWatcher(IAddonLifecycle addonLifecycle, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "Shop", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ShopExchangeItem", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "ShopExchangeCurrency", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "GoldSaucerExchange", OnAddonPostSetup);
    }

    private unsafe void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if child lock is enabled
        if (!configuration.ChildLockEnabled)
        {
            return;
        }

        // Close the vendor addon to block interaction
        var addonPtr = args.Addon;
        var addon = (AtkUnitBase*)(nint)addonPtr;
        if (addon == null)
        {
            return;
        }

        addon->Close(true);
        chatGui.Print("Vendor interaction blocked (Child Lock enabled)");
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

