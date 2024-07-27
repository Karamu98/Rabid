using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RabidPlugin.Windows;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System.Threading;
using DrahsidLib;
using Dalamud.Game.Config;
using Dalamud.Game.ClientState.Conditions;

namespace RabidPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/rabid";
    private const string CommandSettings = "/rabidsys";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RabidPlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        PluginInterface.Create<Service>();
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);


        CommandManager.AddHandler(CommandName, new CommandInfo(OnMainCommand)
        {
            HelpMessage = "Do it."
        });
        CommandManager.AddHandler(CommandSettings, new CommandInfo(OnSettingsCommand)
        {
            HelpMessage = "Settings!"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnMainCommand(string command, string args) => ToggleMainUI();

    private void OnSettingsCommand(string command, string args) => ToggleConfigUI();

    private void DrawUI()
    {
        WindowSystem.Draw();
        DoFPFOVAdjustments();
        DoTPOffset();
    }

    private void DoTPOffset()
    {
        float converted = Configuration.ThirdPersonCombatOffset / 100f * (0.21f - (-0.08f)) + (-0.08f);
        
        if(!Configuration.AlwaysCombatOffset)
        {
            if (!Service.Condition[ConditionFlag.InCombat])
            {
                converted = Configuration.ThirdPersonNoCombatOffset / 100f * (0.21f - (-0.08f)) + (-0.08f);
            }
        }
        Service.GameConfig.Set(UiControlOption.TiltOffset, converted);
    }

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
            if (currentMode == 1)
            {
                active->AddedFoV = 0;
            }
            else
            {
                active->AddedFoV = Configuration.CameraFOV;
            }
        }
    }

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
