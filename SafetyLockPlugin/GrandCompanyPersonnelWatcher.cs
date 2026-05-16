using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for Grand Company Personnel Officer SelectString menus and blocks them when Safety Lock is enabled.
/// Covers all three city-state Personnel Officers (Storm/Serpent/Flame) by matching role-keyed option text.
/// </summary>
public sealed class GrandCompanyPersonnelWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    private DateTime lastChatMessageTime = DateTime.MinValue;

    public GrandCompanyPersonnelWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        this.addonLifecycle.RegisterListener(AddonEvent.PostRefresh, "SelectString", OnSelectStringEvent);
    }

    private void OnSelectStringEvent(AddonEvent type, AddonArgs args)
    {
        if (!configuration.LockEnabled || !configuration.BlockGrandCompanyPersonnel)
        {
            return;
        }

        var addon = gameGui.GetAddonByName("SelectString");
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            var unit = (AtkUnitBase*)addon.Address;
            var selectStringAddon = (AddonSelectString*)addon.Address;

            var popupMenu = &selectStringAddon->PopupMenu;
            for (var i = 0; i < popupMenu->EntryCount && i < 32; i++)
            {
                var entryText = popupMenu->EntryNames[i];
                var text = entryText.ToString();

                if (Localization.ContainsAny(text, Localization.GrandCompanyPersonnel.MatchStrings))
                {
                    unit->Close(true);

                    var now = DateTime.UtcNow;
                    if ((now - lastChatMessageTime).TotalSeconds >= 1)
                    {
                        chatGui.Print(Localization.GrandCompanyPersonnel.BlockedMessage);
                        lastChatMessageTime = now;
                    }

                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "SelectString", OnSelectStringEvent);
    }
}
