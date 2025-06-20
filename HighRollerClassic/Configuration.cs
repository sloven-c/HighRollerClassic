using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace HighRollerClassic;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public (string message, MacroCheckbox checkbox) Greeting = ("", new MacroCheckbox());
    public (string message, MacroCheckbox checkbox) Lose = ("", new MacroCheckbox());
    public int MaxBet = 0;
    public Dictionary<int, (string message, MacroCheckbox checkbox)> MultiplierMessages = new();
    public List<int[]> MultiplierSettings { get; set; } = [];
    public int Version { get; set; } = 1;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public struct MacroCheckbox()
    {
        public bool Preview = false;
        public bool Yell = false;
    }
}
