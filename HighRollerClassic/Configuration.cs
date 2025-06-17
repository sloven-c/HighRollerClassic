using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace HighRollerClassic;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public string greetingMessage = "";
    public string loseMessage = "";
    public int MaxBet = 0;
    public Dictionary<int, string> multiplierMessages = new();
    public List<int[]> multiplierSettings { get; set; } = [];
    public int Version { get; set; } = 0;


    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
