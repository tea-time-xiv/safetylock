﻿using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Child Lock Configuration###ChildLockConfigWindow")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

        Size = new Vector2(500, 300);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Status display
        ImGui.TextUnformatted("Current Status:");
        ImGui.Spacing();
        
        if (configuration.ChildLockEnabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green
            ImGui.TextUnformatted("Child Lock is ENABLED");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
            ImGui.TextUnformatted("Child Lock is DISABLED");
            ImGui.PopStyleColor();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Control checkbox
        var childLockEnabled = configuration.ChildLockEnabled;
        if (ImGui.Checkbox("Enable Child Lock", ref childLockEnabled))
        {
            configuration.ChildLockEnabled = childLockEnabled;
            configuration.Save();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Help text
        ImGui.TextUnformatted("What does Child Lock block?");
        ImGui.Spacing();
        
        ImGui.Indent();
        ImGui.TextUnformatted("• Vendors");
        ImGui.TextUnformatted("• Duty Finder");
        ImGui.TextUnformatted("• Quest acceptance / completion");
        ImGui.Unindent();
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        ImGui.TextUnformatted("Alternative control:");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextUnformatted("/childlockspam can also toggle the lock");
        ImGui.TextUnformatted("(requires 5 executions within 5 seconds)");
        ImGui.Unindent();
    }
}
