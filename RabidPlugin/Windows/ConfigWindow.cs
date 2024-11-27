using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RabidPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private RabidPlugin m_RabidPlugin;
    private Configuration m_Configuration;


    public ConfigWindow(RabidPlugin plugin) : base("Rabid Config###With a constant ID")
    {
        //Flags = ImGuiWindowFlags.AlwaysAutoResize;

        //Size = new Vector2(550, 70);
        //SizeCondition = ImGuiCond.FirstUseEver;

        m_RabidPlugin = plugin;
        m_Configuration = m_RabidPlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("ToolTabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton | ImGuiTabBarFlags.Reorderable))
        {
            //ImGui.Separator();
            //ImGui.Spacing();

            if(ImGui.BeginTabItem("General"))
            {
                if (ImGui.TreeNode("First person FOV adjustment"))
                {
                    ImGui.Checkbox("Enabled", ref m_Configuration.FirstPersonFOVAdjuster);
                    ImGui.Checkbox("Toggle Auto-Face Target", ref m_Configuration.Toggle_AutoFaceTargetWhenUsingAction);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("If \"Automatically face target when using action.\" is enabled, disable this setting when entering first person.\nAuto re-enable it when coming out of first person.");
                        ImGui.EndTooltip();
                    }
                    ImGui.DragFloat("First person FOV Adjustment", ref m_Configuration.CameraFOV, 0.01f, 0.0f, 1.0f);
                    ImGui.Unindent();

                    ImGui.TreePop();
                }
                ImGui.EndTabItem();
            }

            if(ImGui.BeginTabItem("Settings Manager"))
            {
                m_RabidPlugin.SettingsManager.DrawEditor();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}
