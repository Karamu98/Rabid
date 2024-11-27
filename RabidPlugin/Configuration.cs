using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace RabidPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool FirstPersonFOVAdjuster = true;
    public bool Toggle_AutoFaceTargetWhenUsingAction = true;
    public float CameraFOV = 0.4f;
    public SettingsManager.SettingsProfiles SettingsProfiles = new SettingsManager.SettingsProfiles();

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        RabidPlugin.PluginInterface.SavePluginConfig(this);
    }
}
