using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    private const string CommandName = "/pmycommand";
    private const string ChildLockCommandName = "/childlock";
    private const string ChildLockSpamCommandName = "/childlockspam";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private VendorWatcher VendorWatcher { get; init; }
    private DutyFinderWatcher DutyFinderWatcher { get; init; }
    private QuestWatcher QuestWatcher { get; init; }

    // Track spam command executions
    private readonly List<DateTime> _spamCommandTimestamps = new();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Apply EnableOnStartup setting (runtime-only; do not persist here)
        if (Configuration.EnableOnStartup)
        {
            Configuration.ChildLockEnabled = true;
        }

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        // Initialize vendor watcher
        VendorWatcher = new VendorWatcher(AddonLifecycle, ChatGui, Configuration);

        // Initialize duty finder watcher
        DutyFinderWatcher = new DutyFinderWatcher(AddonLifecycle, ChatGui, Configuration);

        // Initialize quest watcher
        QuestWatcher = new QuestWatcher(AddonLifecycle, ChatGui, Configuration);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        CommandManager.AddHandler(ChildLockCommandName, new CommandInfo(OnChildLockCommand)
        {
            HelpMessage = "Toggle child lock"
        });

        CommandManager.AddHandler(ChildLockSpamCommandName, new CommandInfo(OnChildLockSpamCommand)
        {
            HelpMessage = "Toggle child lock (requires 5 executions within 5 seconds)"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
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

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(ChildLockCommandName);
        CommandManager.RemoveHandler(ChildLockSpamCommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }

    private void OnChildLockCommand(string command, string args)
    {
        // Toggle child lock
        Configuration.ChildLockEnabled = !Configuration.ChildLockEnabled;
        
        // Save the config
        Configuration.Save();
        
        // Print chat message
        var status = Configuration.ChildLockEnabled ? "Enabled" : "Disabled";
        ChatGui.Print($"Child Lock: {status}");
    }

    private void OnChildLockSpamCommand(string command, string args)
    {
        var now = DateTime.UtcNow;
        var fiveSecondsAgo = now.AddSeconds(-5);
        
        // Remove timestamps older than 5 seconds
        _spamCommandTimestamps.RemoveAll(t => t < fiveSecondsAgo);
        
        // Add current timestamp
        _spamCommandTimestamps.Add(now);
        
        // Check if we have 5 executions within 5 seconds
        if (_spamCommandTimestamps.Count >= 5)
        {
            // Toggle child lock
            Configuration.ChildLockEnabled = !Configuration.ChildLockEnabled;
            
            // Save the config
            Configuration.Save();
            
            // Clear timestamps
            _spamCommandTimestamps.Clear();
            
            // Print chat message
            var status = Configuration.ChildLockEnabled ? "Enabled" : "Disabled";
            ChatGui.Print($"Child Lock: {status}");
        }
        // If fewer than 5, do nothing (no chat message)
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
