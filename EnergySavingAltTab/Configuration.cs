using Dalamud.Configuration;
using System;

namespace EnergySavingAltTab;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; }

    public bool DisableWhenActivityDetected { get; set; }

    public int FramesPerTenSeconds { get; set; } = 160;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
