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
    
    // Debounce flag to prevent repeated blocking and chat spam
    private bool hasBlockedLeve;

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
        // Reset debounce flag if Child Safety Lock is disabled
        if (!configuration.ChildLockEnabled || !configuration.BlockLevequests)
        {
            hasBlockedLeve = false;
            return;
        }

        // Check if both addons are closed - if so, reset debounce flag
        var guildLeveAddon = gameGui.GetAddonByName("GuildLeve");
        var leveQuestDetailAddon = gameGui.GetAddonByName("LeveQuestDetail");
        
        if (guildLeveAddon.Address == nint.Zero && leveQuestDetailAddon.Address == nint.Zero)
        {
            hasBlockedLeve = false;
            return;
        }

        // If already blocked, don't block again
        if (hasBlockedLeve)
        {
            return;
        }

        // Get the current addon pointer
        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        // Close the levequest addon to block interaction
        unsafe
        {
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        
        // Set debounce flag
        hasBlockedLeve = true;
        
        chatGui.Print("Levequest acceptance blocked (Child Safety Lock enabled)");
    }


    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "GuildLeve", OnAddonPostSetup);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "LeveQuestDetail", OnAddonPostSetup);
    }
}

