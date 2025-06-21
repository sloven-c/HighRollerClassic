using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace HighRollerClassic;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public (string message, DataStructures.MacroCheckbox checkbox) Greeting = ("", new DataStructures.MacroCheckbox());
    public (string message, DataStructures.MacroCheckbox checkbox) Lose = ("", new DataStructures.MacroCheckbox());
    public Dictionary<int, (string message, DataStructures.MacroCheckbox checkbox)> MacroSettings = new();
    public int MaxBet = 0;
    public List<DataStructures.MpSettings> MultiplierSettings = [];
    public int Version { get; set; } = 1;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
