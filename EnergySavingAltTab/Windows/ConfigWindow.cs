using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using System;

namespace EnergySavingAltTab.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Energy Saving Alt-Tab Configuration")
    {
        configuration = plugin.Configuration;
    }

    public void Dispose()
    { }

    public override void Draw()
    {
        bool enabled = configuration.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            configuration.Enabled = enabled;
            configuration.Save();
        }
        ImGuiComponents.HelpMarker("Enable the plugin");

        bool disableOnActivity = configuration.DisableWhenActivityDetected;

        uint frames = (uint)configuration.FramesPerTenSeconds;
        if (ImGui.DragUInt("Frames per ten seconds", ref frames, 1, 1, 160))
        {
            configuration.FramesPerTenSeconds = (int)frames;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker("The framerate to slow down to when the window is not active. The vanilla limiter sets this as 160");

        bool disableWhenCrafting = configuration.DisableWhenCrafting;
        if (ImGui.Checkbox("Disable when crafting", ref disableWhenCrafting))
        {
            configuration.DisableWhenCrafting = disableWhenCrafting;
            configuration.Save();
        }
        ImGuiComponents.HelpMarker("I'm not sure if low framerate can actually affect the crafting product, so here's the option to disable it when crafting.");
    }
}
