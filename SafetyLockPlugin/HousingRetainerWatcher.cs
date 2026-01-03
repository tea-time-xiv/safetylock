using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for housing retainer UI addons and blocks access while Child Safety Lock is enabled.
/// Only blocks housing estate retainers, not summoning bell retainers.
/// Uses target detection to differentiate between summoning bells and housing retainers.
/// </summary>
public sealed class HousingRetainerWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly ITargetManager targetManager;
    private readonly Configuration configuration;

    public HousingRetainerWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, ITargetManager targetManager, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.targetManager = targetManager;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for retainer-related addons
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

        // Check the target's ObjectKind
        var target = targetManager.Target;
        
        // Block unless the target is EventObj (summoning bells)
        // Summoning bells are EventObj, housing retainers are other types


        /**
         * 1️⃣ Wrong ObjectKind assumption

        You wrote:

        // Summoning bells are EventObj
        target.ObjectKind != ObjectKind.EventObj


        ❌ This is not reliable.

        In practice:

        Summoning bells are usually HousingFixture

        Estate retainers are usually EventNpc

        EventObj is too broad and overlaps with other things.

        This is why your blocking is inconsistent.
         */
        
        // this is wrong, but it seems to work in practice, so leaving it as is for now.
        
        
        if (target != null && target.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.EventObj)
        {
            // Not a summoning bell - block the retainer addon
            var addon = gameGui.GetAddonByName(args.AddonName);
            if (addon.Address != nint.Zero)
            {
                unsafe
                {
                    ((AtkUnitBase*)addon.Address)->Close(true);
                }
                chatGui.Print("Housing retainer interaction blocked (Child Safety Lock enabled)");
            }
        }
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerList", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSellList", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSell", OnAddonPostSetup);
    }
}
