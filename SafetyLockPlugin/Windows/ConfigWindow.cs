using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SafetyLockPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Safety Lock Configuration###SafetyLockConfigWindow")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

        Size = new Vector2(500, 300);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Current Status:");
        ImGui.Spacing();

        if (configuration.LockEnabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            ImGui.TextUnformatted("Safety Lock is ENABLED");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            ImGui.TextUnformatted("Safety Lock is DISABLED");
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var lockEnabled = configuration.LockEnabled;
        if (ImGui.Checkbox("Enable Safety Lock", ref lockEnabled))
        {
            configuration.LockEnabled = lockEnabled;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var enableOnStartup = configuration.EnableOnStartup;
        if (ImGui.Checkbox("Enable Safety Lock on startup", ref enableOnStartup))
        {
            configuration.EnableOnStartup = enableOnStartup;
            configuration.Save();
        }

        ImGui.TextWrapped("Automatically enables Safety Lock when the plugin loads.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted("Blocked Features:");
        ImGui.Spacing();

        // Feature checkboxes - only editable when Safety Lock is enabled
        if (!configuration.LockEnabled)
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

        var blockHousingPictureFrames = configuration.BlockHousingPictureFrames;
        if (ImGui.Checkbox("Block Housing Picture Frames", ref blockHousingPictureFrames))
        {
            configuration.BlockHousingPictureFrames = blockHousingPictureFrames;
            configuration.Save();
        }

        if (!configuration.LockEnabled)
        {
            ImGui.EndDisabled();
        }

        ImGui.Spacing();
    }
}
