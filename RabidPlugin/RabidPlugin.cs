using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RabidPlugin.Windows;
using System;
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
    public ConfigWindow RabidWindow;

    public SettingsManager SettingsManager { get; private set; }

    private Tuple<string, CommandInfo>[] m_Commands;


    public RabidPlugin()
    {
        PluginInterface.Create<Service>();
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        RabidWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(RabidWindow);

        m_Commands =
        [
            new ("/rabidconf", new CommandInfo(OnSettingsCommand){ HelpMessage = "Settings!" } ),
        ];
        foreach (var m in m_Commands)
        {
            CommandManager.AddHandler(m.Item1, m.Item2);
        }

        SettingsManager = new SettingsManager(Configuration);
        SettingsManager.MapSettings();

        PluginInterface.UiBuilder.Draw += UIUpdate;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        RabidWindow.Dispose();

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

    public void ToggleConfigUI() => RabidWindow.Toggle();
}
