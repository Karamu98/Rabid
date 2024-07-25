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

namespace RabidPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("BARK BARK BARK##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    static string[] ModeStrings = { "FirstPerson", "ThirdPerson"};
    public override void Draw()
    {
        ImGui.Text("idk what the fuck to put in here");

        unsafe
        {
            GameCamera* active = GameCameraManager.Instance()->Camera;
            int currentMode = active->Mode;

            ImGui.Text($"Current Mode: {ModeStrings[currentMode]}");
            ImGui.Text($"FOV: {active->FoV}");
            ImGui.Text($"Added FOV: {active->AddedFoV}");
        }
        return;
    }
}
