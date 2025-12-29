using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SafetyLockPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Child Safety Lock##ChildLockMainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(500, 300)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Child Safety Lock Plugin");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Current status
        ImGui.TextUnformatted("Current Status:");
        if (plugin.Configuration.ChildLockEnabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green
            ImGui.TextUnformatted("ENABLED");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
            ImGui.TextUnformatted("DISABLED");
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Quick toggle
        if (ImGui.Button("Toggle Child Safety Lock"))
        {
            plugin.Configuration.ChildLockEnabled = !plugin.Configuration.ChildLockEnabled;
            plugin.Configuration.Save();
        }

        ImGui.Spacing();

        // Settings button
        if (ImGui.Button("Open Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Info
        ImGui.TextWrapped("Use /childlock to quickly toggle the Child Safety Lock on/off.");
    }
}

