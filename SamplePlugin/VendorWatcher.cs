using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

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
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Only react if child lock is enabled
        if (!configuration.ChildLockEnabled)
        {
            return;
        }

        // Report vendor interaction
        chatGui.Print("Vendor interaction detected (Child Lock enabled)");
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Shop", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ShopExchangeItem", OnAddonPostSetup);
    }
}

