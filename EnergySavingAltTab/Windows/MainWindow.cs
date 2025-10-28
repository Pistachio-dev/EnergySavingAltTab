using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;

namespace EnergySavingAltTab.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base($"Energy Saving Alt-Tab Main Window")
    {
        this.plugin = plugin;
    }

    public void Dispose()
    { }

    public override void Draw()
    {
        if (ImGui.Button("Open configuration"))
        {
            plugin.ToggleConfigUi();
            plugin.ToggleMainUi();
        }
    }
}
