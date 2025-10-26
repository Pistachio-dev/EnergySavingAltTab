using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace EnergySavingAltTab.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Configuration")
    {       
        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
    }
}
