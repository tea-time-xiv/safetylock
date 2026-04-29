using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SafetyLockPlugin.Windows;
using System;
using System.Collections.Generic;

namespace SafetyLockPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

    private const string SafetyLockCommandName = "/safetylock";
    private const string SafetyLockSpamCommandName = "/safetylockspam";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SafetyLock");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private VendorWatcher VendorWatcher { get; init; }
    private DutyFinderWatcher DutyFinderWatcher { get; init; }
    private QuestWatcher QuestWatcher { get; init; }
    private GlamourDresserWatcher GlamourDresserWatcher { get; init; }
    private ArmoireWatcher ArmoireWatcher { get; init; }
    private FreeCompanyChestWatcher FreeCompanyChestWatcher { get; init; }
    private HousingFoodWatcher HousingFoodWatcher { get; init; }
    private LevequestWatcher LevequestWatcher { get; init; }
    private HousingRetainerWatcher HousingRetainerWatcher { get; init; }
    private AdditionalChambersWatcher AdditionalChambersWatcher { get; init; }
    private HousingPictureFrameWatcher HousingPictureFrameWatcher { get; init; }
    private LevemeteMenuWatcher LevemeteMenuWatcher { get; init; }

    // Track spam command executions
    private readonly List<DateTime> spamCommandTimestamps = new();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Apply EnableOnStartup setting (runtime-only; do not persist here)
        if (Configuration.EnableOnStartup)
        {
            Configuration.LockEnabled = true;
        }

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        VendorWatcher = new VendorWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        DutyFinderWatcher = new DutyFinderWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        QuestWatcher = new QuestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        GlamourDresserWatcher = new GlamourDresserWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        ArmoireWatcher = new ArmoireWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        FreeCompanyChestWatcher = new FreeCompanyChestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        HousingFoodWatcher = new HousingFoodWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        LevequestWatcher = new LevequestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        HousingRetainerWatcher = new HousingRetainerWatcher(AddonLifecycle, GameGui, ChatGui, TargetManager, Configuration);
        AdditionalChambersWatcher = new AdditionalChambersWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        HousingPictureFrameWatcher = new HousingPictureFrameWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);
        LevemeteMenuWatcher = new LevemeteMenuWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        CommandManager.AddHandler(SafetyLockCommandName, new CommandInfo(OnSafetyLockCommand)
        {
            HelpMessage = "Toggle Safety Lock"
        });

        CommandManager.AddHandler(SafetyLockSpamCommandName, new CommandInfo(OnSafetyLockSpamCommand)
        {
            HelpMessage = "Toggle Safety Lock (requires 5 executions within 5 seconds)"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Information("Safety Lock plugin loaded successfully");
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        VendorWatcher.Dispose();
        DutyFinderWatcher.Dispose();
        QuestWatcher.Dispose();
        GlamourDresserWatcher.Dispose();
        ArmoireWatcher.Dispose();
        FreeCompanyChestWatcher.Dispose();
        HousingFoodWatcher.Dispose();
        LevequestWatcher.Dispose();
        HousingRetainerWatcher.Dispose();
        AdditionalChambersWatcher.Dispose();
        HousingPictureFrameWatcher.Dispose();
        LevemeteMenuWatcher.Dispose();

        CommandManager.RemoveHandler(SafetyLockCommandName);
        CommandManager.RemoveHandler(SafetyLockSpamCommandName);
    }

    private void OnSafetyLockCommand(string command, string args)
    {
        Configuration.LockEnabled = !Configuration.LockEnabled;
        Configuration.Save();
        var status = Configuration.LockEnabled ? "Enabled" : "Disabled";
        ChatGui.Print($"Safety Lock: {status}");
    }

    private void OnSafetyLockSpamCommand(string command, string args)
    {
        var now = DateTime.UtcNow;
        var fiveSecondsAgo = now.AddSeconds(-5);

        spamCommandTimestamps.RemoveAll(t => t < fiveSecondsAgo);
        spamCommandTimestamps.Add(now);

        if (spamCommandTimestamps.Count >= 5)
        {
            Configuration.LockEnabled = !Configuration.LockEnabled;
            Configuration.Save();
            spamCommandTimestamps.Clear();
            var status = Configuration.LockEnabled ? "Enabled" : "Disabled";
            ChatGui.Print($"Safety Lock: {status}");
        }
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
