using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for housing food consumption confirmation dialogs and blocks them when Child Safety Lock is enabled.
/// </summary>
public sealed class HousingFoodWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public HousingFoodWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        // Subscribe to SelectYesno addon - PostRefresh is more reliable as text is populated after PostSetup
        this.addonLifecycle.RegisterListener(AddonEvent.PostRefresh, "SelectYesno", OnAddonEvent);
        // Also listen on PostSetup in case text is ready early
        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", OnAddonEvent);
    }

    private void OnAddonEvent(AddonEvent type, AddonArgs args)
    {
        // Only react if Child Safety Lock is enabled and housing food blocking is enabled
        if (!configuration.ChildLockEnabled || !configuration.BlockHousingFood)
        {
            return;
        }

        // Get the addon pointer
        var addon = gameGui.GetAddonByName("SelectYesno");
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            var unit = (AtkUnitBase*)addon.Address;
            
            // Scan all text nodes to find the dialog text
            for (var i = 0; i < unit->UldManager.NodeListCount; i++)
            {
                var node = unit->UldManager.NodeList[i];
                if (node == null || node->Type != NodeType.Text)
                {
                    continue;
                }
                
                var textNode = (AtkTextNode*)node;
                
                // Read raw UTF-8 string pointer properly
                var stringPtr = (byte*)textNode->NodeText.StringPtr;
                if (stringPtr == null)
                {
                    continue;
                }
                
                var text = Marshal.PtrToStringUTF8((nint)stringPtr) ?? string.Empty;
                
                // Normalize the text: trim whitespace
                var normalized = text.Trim();
                
                // Check if this text node starts with "Partake of the" (handle both correct spelling and typo variant)
                if (normalized.StartsWith("Partake of the", StringComparison.OrdinalIgnoreCase) ||
                    normalized.StartsWith("Parttake of the", StringComparison.OrdinalIgnoreCase))
                {
                    // This is a housing food confirmation - close it
                    unit->Close(true);
                    chatGui.Print("Housing food consumption blocked (Child Safety Lock enabled)");
                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "SelectYesno", OnAddonEvent);
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "SelectYesno", OnAddonEvent);
    }
}

