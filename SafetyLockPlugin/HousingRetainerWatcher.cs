using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SafetyLockPlugin;

/// <summary>
/// Watches the RetainerList addon (summoning bell / housing retainer menu) and closes it while Safety Lock is enabled.
/// </summary>
public sealed class HousingRetainerWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public HousingRetainerWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerList", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        if (!configuration.LockEnabled || !configuration.BlockHousingRetainers)
        {
            return;
        }

        var addon = gameGui.GetAddonByName(args.AddonName);
        if (addon.Address == nint.Zero)
        {
            return;
        }

        unsafe
        {
            ((AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print(Localization.HousingRetainer.BlockedMessage);
    }

    public void Dispose()
    {
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerList", OnAddonPostSetup);
    }
}
