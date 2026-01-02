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

    private const string ChildLockCommandName = "/childlock";
    private const string ChildLockSpamCommandName = "/childlockspam";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("ChildLock");
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

    // Track spam command executions
    private readonly List<DateTime> spamCommandTimestamps = new();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Apply EnableOnStartup setting (runtime-only; do not persist here)
        if (Configuration.EnableOnStartup)
        {
            Configuration.ChildLockEnabled = true;
        }

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        // Initialize vendor watcher
        VendorWatcher = new VendorWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize duty finder watcher
        DutyFinderWatcher = new DutyFinderWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize quest watcher
        QuestWatcher = new QuestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize glamour dresser watcher
        GlamourDresserWatcher = new GlamourDresserWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize armoire watcher
        ArmoireWatcher = new ArmoireWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize free company chest watcher
        FreeCompanyChestWatcher = new FreeCompanyChestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize housing food watcher
        HousingFoodWatcher = new HousingFoodWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        // Initialize levequest watcher
        LevequestWatcher = new LevequestWatcher(AddonLifecycle, GameGui, ChatGui, Configuration);

        CommandManager.AddHandler(ChildLockCommandName, new CommandInfo(OnChildLockCommand)
        {
            HelpMessage = "Toggle Child Safety Lock"
        });

        CommandManager.AddHandler(ChildLockSpamCommandName, new CommandInfo(OnChildLockSpamCommand)
        {
            HelpMessage = "Toggle Child Safety Lock (requires 5 executions within 5 seconds)"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log
        Log.Information("Child Safety Lock plugin loaded successfully");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
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

        CommandManager.RemoveHandler(ChildLockCommandName);
        CommandManager.RemoveHandler(ChildLockSpamCommandName);
    }

    private void OnChildLockCommand(string command, string args)
    {
        // Toggle Child Safety Lock
        Configuration.ChildLockEnabled = !Configuration.ChildLockEnabled;
        
        // Save the config
        Configuration.Save();
        
        // Print chat message
        var status = Configuration.ChildLockEnabled ? "Enabled" : "Disabled";
        ChatGui.Print($"Child Safety Lock: {status}");
    }

    private void OnChildLockSpamCommand(string command, string args)
    {
        var now = DateTime.UtcNow;
        var fiveSecondsAgo = now.AddSeconds(-5);
        
        // Remove timestamps older than 5 seconds
        spamCommandTimestamps.RemoveAll(t => t < fiveSecondsAgo);
        
        // Add current timestamp
        spamCommandTimestamps.Add(now);
        
        // Check if we have 5 executions within 5 seconds
        if (spamCommandTimestamps.Count >= 5)
        {
            // Toggle Child Safety Lock
            Configuration.ChildLockEnabled = !Configuration.ChildLockEnabled;
            
            // Save the config
            Configuration.Save();
            
            // Clear timestamps
            spamCommandTimestamps.Clear();
            
            // Print chat message
            var status = Configuration.ChildLockEnabled ? "Enabled" : "Disabled";
            ChatGui.Print($"Child Safety Lock: {status}");
        }
        // If fewer than 5, do nothing (no chat message)
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}

