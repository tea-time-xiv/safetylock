﻿using Dalamud.Configuration;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    // Child Lock
    public bool ChildLockEnabled { get; set; } = false;
    public bool EnableOnStartup { get; set; } = false;

    // Feature toggles
    public bool BlockVendors { get; set; } = true;
    public bool BlockDutyFinder { get; set; } = true;
    public bool BlockQuests { get; set; } = true;

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
