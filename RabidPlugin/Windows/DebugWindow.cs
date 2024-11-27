using System;
using System.Runtime.InteropServices;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Configuration;
using System.Text;
using RabidPlugin.Extern;

namespace RabidPlugin.Windows;

public class DebugWindow : Window, IDisposable
{
    public DebugWindow(RabidPlugin plugin) : base("Debug##Rabid")
    {
        m_Plugin = plugin;
    }
    public void Dispose() { }

    public override void Draw()
    {
        if(ImGui.Button("Settings print"))
        {
            m_Plugin.SettingsManager.DebugSettings(true);
        }
        if (ImGui.BeginChild("Debug"))
        {
            Helpers.CollapsingTreeNode("Debug", new Helpers.TreeFrame(), (col) =>
            {
                Helpers.CollapsingTreeNode("Camera", col, (col) =>
                {
                    unsafe
                    {
                        GameCameraManager* man = GameCameraManager.Instance();
                        ImGui.Text($"Active camera index: {man->ActiveCameraIndex}");
                        ImGui.Text($"Previous camera index: {man->PreviousCameraIndex}");
                        Helpers.CollapsingTreeNode("Active Camera", col, (col) =>
                        {
                            GameCamera* active = null;
                            switch (man->ActiveCameraIndex)
                            {
                                case 0: { active = man->Camera; break; }
                                case 2: { active = &man->LobbCamera->Camera; break; }
                                case 3: { active = &man->Camera3->Camera; break; }
                            }

                            if (active != null)
                            {
                                ImDrawGameCamera(active);
                            }
                            else
                            {
                                ImGui.Text("Active camera Null...");
                            }
                        });

                        Helpers.CollapsingTreeNode("Other Cameras", col, (col) =>
                        {
                            Helpers.CollapsingTreeNode("World Camera (0)", col, (col) =>ImDrawGameCamera(man->Camera));
                            Helpers.CollapsingTreeNode("Lobby Camera (2)", col, (col) =>ImDrawGameCamera(&man->LobbCamera->Camera));
                            Helpers.CollapsingTreeNode("Spectator Camera (3)", col, (col) => ImDrawGameCamera(&man->Camera3->Camera));
                        });
                    }
                });
                DrawAllSysConfig(col);

            });

            ImGui.EndChild();
        }
        
    }

    private unsafe void DrawAllSysConfig(Helpers.TreeFrame col)
    {
        Helpers.CollapsingTreeNode("System Config", col, (col) =>
        {
            ImGui.Checkbox("Show NULL", ref m_ShowNullSysNames);
            unsafe
            {
                SystemConfig* config = &Framework.Instance()->SystemConfig;
                if (config != null)
                {
                    for (int i = 0; i < config->ConfigCount; ++i)
                    {
                        FFXIVClientStructs.FFXIV.Common.Configuration.ConfigEntry* entry = &config->ConfigEntry[i];
                        if (entry == null)
                        {
                            continue;
                        }

                        string? name = Marshal.PtrToStringUTF8((nint)entry->Name);
                        if (name == null && !m_ShowNullSysNames)
                        {
                            continue;
                        }

                        name = name == null ? "NULL" : name;
                        string label = $"{name}##Sys_Config_{i}";
                        Helpers.CollapsingTreeNode(label, col, (col) =>
                        {
                            string? val = null;
                            var strPtr = entry->Value.String;
                            //ImGui.SameLine(); ImGui.Text($"Type: {entry->Type}");
                            ImGui.SameLine(); ImGui.Text($"idx: {entry->Index}");
                            switch (entry->Type)
                            {
                                case 4:
                                    {
                                        if (strPtr != null)
                                        {
                                            val = Helpers.ConvertBytePointerToString(strPtr->StringPtr);
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        int curVal = (int)entry->Value.UInt;
                                        if (ImGui.InputInt($"##{label}_{entry->Index}", ref curVal))
                                        {
                                            entry->SetValueUInt((uint)curVal);
                                        }
                                        val = entry->Value.UInt.ToString();
                                        break;
                                    }
                                case 3:
                                    {
                                        val = entry->Value.Float.ToString();
                                        break;
                                    }
                                case 1:
                                    {
                                        val = "??? no clue what the fuck this is";
                                        break;
                                    }
                            }

                            val = val == null ? "Failed to parse Value" : val;
                            ImGui.Text(val);
                        });
                    }
                }
            }
        });
    }

    private unsafe void ImDrawGameCamera(GameCamera* targetCam)
    {
        ImGui.Indent();
        ImGui.Text("Position");
        ImGui.Indent();
        ImGui.Text($"X: {targetCam->CameraBase.X:0.000} Y: {targetCam->CameraBase.Y:0.000}, Z: {targetCam->CameraBase.Z:0.000}");
        ImGui.Unindent();
        ImGui.Text("Look at position");
        ImGui.Indent();
        ImGui.Text($"X: {targetCam->CameraBase.LookAtX:0.000} Y: {targetCam->CameraBase.LookAtY:0.000}, Z: {targetCam->CameraBase.LookAtZ:0.000}");
        ImGui.Unindent();

        ImGui.DragFloat("Distance", ref targetCam->Distance, 0.01f, 1.5f, 20.0f);
        ImGui.Text($"Min/Max Distance: {targetCam->MinDistance}/{targetCam->MaxDistance}");

        ImGui.Text($"FOV: {targetCam->FoV}");
        ImGui.Text($"Min/Max FOV: {targetCam->MinFoV}/{targetCam->MaxFoV}");
        ImGui.Text($"Added FOV: {targetCam->AddedFoV}");

        ImGui.DragFloat("Yaw", ref targetCam->Yaw, 0.01f);
        ImGui.Indent();
        ImGui.Text($"Yaw Delta {targetCam->YawDelta}");
        ImGui.Unindent();
        ImGui.DragFloat("Pitch", ref targetCam->Pitch, 0.01f);
        ImGui.Indent();
        ImGui.Text($"Min/Max Pitch: {targetCam->MinPitch}/{targetCam->MaxPitch}");
        ImGui.Unindent();
        ImGui.DragFloat("Roll", ref targetCam->Roll, 0.01f);

        int currentmode = targetCam->Mode;
        ImGui.Text($"Camera control mode: {ModeStrings[currentmode]}");

        int currentControlType = targetCam->ControlType;
        ImGui.Text($"Control Type: {ControlTypeStrings[currentControlType]}");

        ImGui.DragFloat("FP/TP Interpolate Distance", ref targetCam->InterpDistance, 0.01f);
        ImGui.Text($"Save distance: {targetCam->SavedDistance}");
        ImGui.Text($"Transition: {targetCam->Transition}");

        ImGui.Text($"Is Flipped: {targetCam->IsFlipped}");

        if(ImGui.TreeNode("GPose only pan </3"))
        {
            GPoseCamera* newCam = (GPoseCamera*)targetCam;
            ImGui.DragFloat("Pan X", ref newCam->Pan.X, 0.001f);
            ImGui.DragFloat("Pan Y", ref newCam->Pan.Y, 0.001f);
        }
        ImGui.Unindent();
    }


    private RabidPlugin m_Plugin;
    private bool m_ShowNullSysNames = false;

    static readonly string[] ModeStrings = { "FirstPerson", "ThirdPerson" };
    static readonly string[] ControlTypeStrings = { "FirstPerson", "Legacy", "Standard" };
}
