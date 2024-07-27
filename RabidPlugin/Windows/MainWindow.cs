using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using DrahsidLib;
using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Dalamud.Plugin.Services;

namespace RabidPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Rabid##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }
    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginChild("RabidMain"))
        {
            if (ImGui.TreeNode("Debug"))
            {
                if (ImGui.TreeNode("Camera"))
                {
                    unsafe
                    {
                        GameCameraManager* man = GameCameraManager.Instance();
                        ImGui.Text($"Active camera index: {man->ActiveCameraIndex}");
                        ImGui.Text($"Previous camera index: {man->PreviousCameraIndex}");
                        if (ImGui.TreeNode("Active Camera"))
                        {
                            GameCamera* active = null;
                            switch (man->ActiveCameraIndex)
                            {
                                case 0: { active = man->Camera; break; }
                                case 2: { active = &man->LobbCamera->Camera; break; }
                                case 3: { active = &man->Camera3->Camera; break; }
                            }
                            
                            if(active != null)
                            {
                                PrintGameCamera(active);
                            }
                            else
                            {
                                ImGui.Text("Active camera Null...");
                            }
                        }
                        if (ImGui.TreeNode("Other Cameras"))
                        {
                            if (ImGui.TreeNode("World Camera (0)"))
                            {
                                PrintGameCamera(man->Camera);
                            }
                            if (ImGui.TreeNode("Lobby Camera (2)"))
                            {
                                PrintGameCamera(&man->LobbCamera->Camera);
                            }
                            if (ImGui.TreeNode("Spectator Camera (3)"))
                            {
                                PrintGameCamera(&man->Camera3->Camera);
                            }
                        }

                    }
                }
            }

            ImGui.EndChild();
        }
    }

    static string[] ModeStrings = { "FirstPerson", "ThirdPerson" };
    static string[] ControlTypeStrings = { "FirstPerson", "Legacy", "Standard" };
    private unsafe void PrintGameCamera(GameCamera* targetCam)
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
}
