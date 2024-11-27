using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RabidPlugin.Windows;
using System;
using System.Collections.Generic;
using RabidPlugin.Extern;

namespace RabidPlugin;

public sealed class RabidPlugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IPluginLog? Log { get; private set; } = null;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RabidPlugin");
    public enum RabidWindow
    {
        Config,
        Debug,
        COUNT
    };
    public Window[] Windows;

    public SettingsManager SettingsManager { get; private set; }

    private Tuple<string, CommandInfo>[] m_Commands;


    public RabidPlugin()
    {
        PluginInterface.Create<Service>();
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Windows = 
        [
            new ConfigWindow(this),
            new DebugWindow(this),
        ];
        foreach (var window in Windows)
        {
            WindowSystem.AddWindow(window);
        }

        m_Commands =
        [
            new ("/rabid", new CommandInfo(OnMainCommand){ HelpMessage = "Debug Stuff:)" } ),
            new ("/rabidsys", new CommandInfo(OnSettingsCommand){ HelpMessage = "Settings!" } ),
        ];
        foreach (var m in m_Commands)
        {
            CommandManager.AddHandler(m.Item1, m.Item2);
        }

        SettingsManager = new SettingsManager(Configuration);
        SettingsManager.MapSettings();

        PluginInterface.UiBuilder.Draw += UIUpdate;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleDebugUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        foreach (Window window in Windows)
        {
            IDisposable disp = (IDisposable)window;
            disp.Dispose();
        }

        foreach (var command in m_Commands)
        {
            CommandManager.RemoveHandler(command.Item1);
        }
    }

    private void UIUpdate()
    {
        WindowSystem.Draw();

        DoFPFOVAdjustments();
    }

    private void OnMainCommand(string command, string args) => ToggleDebugUI();

    private void OnSettingsCommand(string command, string args) => ToggleConfigUI();


    private void DoFPFOVAdjustments()
    {
        if(!Configuration.FirstPersonFOVAdjuster)
        {
            return;
        }

        unsafe
        {
            GameCamera* active = GameCameraManager.Instance()->Camera;
            int currentMode = active->Mode;
            if (!FFXIVClientStructs.FFXIV.Client.Game.GameMain.IsInGPose())
            {
                if (currentMode == 1/*thirdperson*/)
                {
                    active->AddedFoV = 0;
                }
                else
                {
                    active->AddedFoV = Configuration.CameraFOV;
                }
            }

        }
    }

    public void ToggleConfigUI() => Windows[(int)RabidWindow.Config].Toggle();
    public void ToggleDebugUI() => Windows[(int)RabidWindow.Debug].Toggle();
}
