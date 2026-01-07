using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for "Entrance to Additional Chambers" menu in Free Company houses and blocks it when Child Safety Lock is enabled.
/// </summary>
public sealed class AdditionalChambersWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    
    private DateTime lastChatMessageTime = DateTime.MinValue;

    public AdditionalChambersWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
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
        // Only react if Child Safety Lock is enabled and additional chambers blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockAdditionalChambers)
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
                
                // Check if this entry contains indicators of the additional chambers menu
                if (text.Contains("private chambers", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("company workshop", StringComparison.OrdinalIgnoreCase))
                {
                    // This is the additional chambers menu - close it
                    unit->Close(true);
                    
                    // Print chat message with 1s cooldown
                    var now = DateTime.UtcNow;
                    if ((now - lastChatMessageTime).TotalSeconds >= 1)
                    {
                        chatGui.Print("Additional chambers access blocked (Child Safety Lock enabled)");
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

