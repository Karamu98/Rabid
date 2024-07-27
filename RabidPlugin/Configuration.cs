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

    public bool ThirdPersonOffset = true;
    public Vector2 ThirdPersonNoCombatOffset = new Vector2(0.0f, 0.0f);

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
