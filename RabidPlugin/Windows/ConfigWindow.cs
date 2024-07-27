using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DrahsidLib;
using ImGuiNET;

namespace RabidPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("RABID CONFIG AYAYAYA###With a constant ID")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;

        Size = new Vector2(550, 70);
        SizeCondition = ImGuiCond.FirstUseEver;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if(ImGui.TreeNode("First person FOV adjustment"))
        {
            ImGui.Checkbox("Enabled", ref Configuration.FirstPersonFOVAdjuster);
            ImGui.DragFloat("First person FOV Adjustment", ref Configuration.CameraFOV, 0.01f, 0.0f, 1.0f);
            ImGui.Unindent();
        }
        if (ImGui.TreeNode("Third person camera offset (non-combat)"))
        {
            ImGui.Checkbox("Always combat offset", ref Configuration.AlwaysCombatOffset);
            ImGui.DragFloat("No Combat Offset", ref Configuration.ThirdPersonNoCombatOffset, 1.0f, 0.0f, 100.0f);
            ImGui.DragFloat("Combat Offset", ref Configuration.ThirdPersonCombatOffset, 1.0f, 0.0f, 100.0f);
        }
    }
}
