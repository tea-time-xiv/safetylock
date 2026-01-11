using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for Levemete root SelectString menus and blocks them when Child Safety Lock is enabled.
/// </summary>
public sealed class LevemeteMenuWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    
    private DateTime lastChatMessageTime = DateTime.MinValue;

    public LevemeteMenuWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to SelectString addon with PostRefresh
        this.addonLifecycle.RegisterListener(AddonEvent.PostRefresh, "SelectString", OnSelectStringEvent);
    }

    private void OnSelectStringEvent(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and levequest blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockLevequests)
        {
            return;
        }

        // Get the addon pointer
        var addon = gameGui.GetAddonByName("SelectString");
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            var unit = (AtkUnitBase*)addon.Address;
            var selectStringAddon = (AddonSelectString*)addon.Address;
            
            // Check popup list entries for blocking
            var popupMenu = &selectStringAddon->PopupMenu;
            for (var i = 0; i < popupMenu->EntryCount && i < 32; i++)
            {
                var entryText = popupMenu->EntryNames[i];
                var text = entryText.ToString();
                
                // Check if this entry contains indicators of the Levemete menu
                if (text.Contains("Battlecraft Leves", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Fieldcraft Leves", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Tradecraft Leves", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Information on leves", StringComparison.OrdinalIgnoreCase))
                {
                    // This is the Levemete menu - close it
                    unit->Close(true);
                    
                    // Print chat message with 1s cooldown
                    var now = DateTime.UtcNow;
                    if ((now - lastChatMessageTime).TotalSeconds >= 1)
                    {
                        chatGui.Print("Levequest interaction blocked (Child Safety Lock enabled)");
                        lastChatMessageTime = now;
                    }
                    
                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "SelectString", OnSelectStringEvent);
    }
}

