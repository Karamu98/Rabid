using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace RabidPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public float CameraFOV = 0.4f;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
