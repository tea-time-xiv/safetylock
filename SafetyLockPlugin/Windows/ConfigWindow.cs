﻿﻿using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SafetyLockPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Child Safety Lock Configuration###ChildLockConfigWindow")
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
            ImGui.TextUnformatted("Child Safety Lock is ENABLED");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
            ImGui.TextUnformatted("Child Safety Lock is DISABLED");
            ImGui.PopStyleColor();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Control checkbox
        var childLockEnabled = configuration.ChildLockEnabled;
        if (ImGui.Checkbox("Enable Child Safety Lock", ref childLockEnabled))
        {
            configuration.ChildLockEnabled = childLockEnabled;
            configuration.Save();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Enable on startup checkbox
        var enableOnStartup = configuration.EnableOnStartup;
        if (ImGui.Checkbox("Enable Child Safety Lock on startup", ref enableOnStartup))
        {
            configuration.EnableOnStartup = enableOnStartup;
            configuration.Save();
        }
        
        ImGui.TextWrapped("Automatically enables Child Safety Lock when the plugin loads.");
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Blocked Features section
        ImGui.TextUnformatted("Blocked Features:");
        ImGui.Spacing();
        
        // Feature checkboxes - only editable when Child Safety Lock is enabled
        if (!configuration.ChildLockEnabled)
        {
            ImGui.BeginDisabled();
        }
        
        var blockVendors = configuration.BlockVendors;
        if (ImGui.Checkbox("Block Vendors", ref blockVendors))
        {
            configuration.BlockVendors = blockVendors;
            configuration.Save();
        }
        
        var blockDutyFinder = configuration.BlockDutyFinder;
        if (ImGui.Checkbox("Block Duty Finder", ref blockDutyFinder))
        {
            configuration.BlockDutyFinder = blockDutyFinder;
            configuration.Save();
        }
        
        var blockQuests = configuration.BlockQuests;
        if (ImGui.Checkbox("Block Quests", ref blockQuests))
        {
            configuration.BlockQuests = blockQuests;
            configuration.Save();
        }
        
        var blockGlamourDresser = configuration.BlockGlamourDresser;
        if (ImGui.Checkbox("Block Glamour Dresser", ref blockGlamourDresser))
        {
            configuration.BlockGlamourDresser = blockGlamourDresser;
            configuration.Save();
        }
        
        var blockArmoire = configuration.BlockArmoire;
        if (ImGui.Checkbox("Block Armoire", ref blockArmoire))
        {
            configuration.BlockArmoire = blockArmoire;
            configuration.Save();
        }
        
        var blockFreeCompanyChest = configuration.BlockFreeCompanyChest;
        if (ImGui.Checkbox("Block Free Company Chest", ref blockFreeCompanyChest))
        {
            configuration.BlockFreeCompanyChest = blockFreeCompanyChest;
            configuration.Save();
        }
        
        var blockHousingFood = configuration.BlockHousingFood;
        if (ImGui.Checkbox("Block Housing Food", ref blockHousingFood))
        {
            configuration.BlockHousingFood = blockHousingFood;
            configuration.Save();
        }
        
        var blockLevequests = configuration.BlockLevequests;
        if (ImGui.Checkbox("Block Levequests", ref blockLevequests))
        {
            configuration.BlockLevequests = blockLevequests;
            configuration.Save();
        }
        
        var blockHousingRetainers = configuration.BlockHousingRetainers;
        if (ImGui.Checkbox("Block Housing Retainers", ref blockHousingRetainers))
        {
            configuration.BlockHousingRetainers = blockHousingRetainers;
            configuration.Save();
        }
        
        var blockAdditionalChambers = configuration.BlockAdditionalChambers;
        if (ImGui.Checkbox("Block Additional Chambers", ref blockAdditionalChambers))
        {
            configuration.BlockAdditionalChambers = blockAdditionalChambers;
            configuration.Save();
        }
        
        if (!configuration.ChildLockEnabled)
        {
            ImGui.EndDisabled();
        }
        
        ImGui.Spacing();
    }
}
