using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for levequest UI addons and blocks them when Child Safety Lock is enabled.
/// </summary>
public sealed class LevequestWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    
    // Debounce flags to prevent repeated blocking and chat spam
    private bool hasBlockedGuildLeve;
    private bool hasBlockedLeveQuestDetail;

    public LevequestWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to addon lifecycle events for levequest-related windows
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "GuildLeve", OnAddonPostSetup);
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "LeveQuestDetail", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        // Reset debounce flags if Child Safety Lock is disabled
        if (!configuration.ChildLockEnabled || !configuration.BlockLevequests)
        {
            ResetDebounceFlags();
            return;
        }

        // Get the addon pointer
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            // Addon no longer present - reset debounce flags
            ResetDebounceFlags();
            return;
        }

        // Check debounce flag for this specific addon
        bool hasAlreadyBlocked = args.AddonName switch
        {
            "GuildLeve" => hasBlockedGuildLeve,
            "LeveQuestDetail" => hasBlockedLeveQuestDetail,
            _ => false
        };

        if (hasAlreadyBlocked)
        {
            return;
        }

        // Close the levequest addon to block interaction
        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        
        // Set debounce flag for this addon
        switch (args.AddonName)
        {
            case "GuildLeve":
                hasBlockedGuildLeve = true;
                break;
            case "LeveQuestDetail":
                hasBlockedLeveQuestDetail = true;
                break;
        }
        
        chatGui.Print("Levequest acceptance blocked (Child Safety Lock enabled)");
    }

    private void ResetDebounceFlags()
    {
        hasBlockedGuildLeve = false;
        hasBlockedLeveQuestDetail = false;
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "GuildLeve", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "LeveQuestDetail", OnAddonPostSetup);
    }
}

