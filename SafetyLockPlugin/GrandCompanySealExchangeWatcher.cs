using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;

namespace SafetyLockPlugin;

/// <summary>
/// Watches for the Grand Company Quartermaster seal-exchange window and blocks it when Safety Lock is enabled.
/// </summary>
public sealed class GrandCompanySealExchangeWatcher : IDisposable
{
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IGameGui gameGui;
    private readonly IChatGui chatGui;
    private readonly Configuration configuration;

    public GrandCompanySealExchangeWatcher(IAddonLifecycle addonLifecycle, IGameGui gameGui, IChatGui chatGui, Configuration configuration)
    {
        this.addonLifecycle = addonLifecycle;
        this.gameGui = gameGui;
        this.chatGui = chatGui;
        this.configuration = configuration;

        this.addonLifecycle.RegisterListener(AddonEvent.PostSetup, "GrandCompanyExchange", OnAddonPostSetup);
    }

    private void OnAddonPostSetup(AddonEvent type, AddonArgs args)
    {
        if (!configuration.LockEnabled || !configuration.BlockGrandCompanySealExchange)
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
            ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon.Address)->Close(true);
        }
        chatGui.Print(Localization.GrandCompanySealExchange.BlockedMessage);
    }

    public void Dispose()
    {
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, "GrandCompanyExchange", OnAddonPostSetup);
    }
}
