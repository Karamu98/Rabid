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
    public float CameraFOV = 0.4f;

    public bool TPOffsetEnabled = false;
    public bool AlwaysCombatOffset = false;
    public float ThirdPersonNoCombatOffset = 30.0f;
    public float ThirdPersonCombatOffset = 0.0f;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
