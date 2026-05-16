using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for housing picture frame menus and blocks them when Safety Lock is enabled.
/// </summary>
public sealed class HousingPictureFrameWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;
    
    private DateTime lastChatMessageTime = DateTime.MinValue;

    public HousingPictureFrameWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
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
        // Only react if Safety Lock is enabled and picture frame blocking is enabled
        if (!configuration.LockEnabled || !configuration.BlockHousingPictureFrames)
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
                
                if (Localization.ContainsAny(text, Localization.HousingPictureFrame.MatchStrings))
                {
                    unit->Close(true);

                    var now = DateTime.UtcNow;
                    if ((now - lastChatMessageTime).TotalSeconds >= 1)
                    {
                        chatGui.Print(Localization.HousingPictureFrame.BlockedMessage);
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

