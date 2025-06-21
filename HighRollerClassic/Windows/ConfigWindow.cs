using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HighRollerClassic.Windows;

public class ConfigWindow : Window, IDisposable
{
    private const int MinRoll = 1, MaxRoll = 999;
    private readonly string[] comparators;
    private readonly Configuration Configuration;
    private readonly Plugin Plugin;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("HRC Settings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(250, 220);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        comparators = ["=", ">="];
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Add new...") || Configuration.MultiplierSettings.Count == 0) AddMultiplier();
        ImGui.SameLine();
        if (ImGui.Button("Macro settings")) Plugin.ToggleMacroUI();
        ImGui.Spacing();

        const float width = 50f;
        for (var i = 0; i < Configuration.MultiplierSettings.Count; i++)
        {
            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##multiplier{i}", ref Configuration.MultiplierSettings[i].multiplier, 0);
            Configuration.MultiplierSettings[i].multiplier =
                VerifyValue(Configuration.MultiplierSettings[i].multiplier);

            ImGui.SameLine();

            ImGui.Text("x");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##roll{i}", ref Configuration.MultiplierSettings[i].roll, 0);
            Configuration.MultiplierSettings[i].roll = VerifyValue(Configuration.MultiplierSettings[i].roll);

            ImGui.SameLine();

            ImGui.SetNextItemWidth(width * 1.1f);
            var comparator = (int)Configuration.MultiplierSettings[i].comparator;
            if (ImGui.Combo($"##comparators{i}", ref comparator, comparators,
                            comparators.Length))
                Configuration.MultiplierSettings[i].comparator = (DataStructures.Comparators)comparator;

            ImGui.SameLine();
            if (Configuration.MultiplierSettings.Count > 1)
            {
                if (ImGui.Button($"-##{i}"))
                    RemoveMultiplier(i);
            }

            ImGui.SameLine();
            var colour = Configuration.MultiplierSettings[i].colour;
            var newColour =
                ImGuiComponents.ColorPickerWithPalette(
                    i, $"{Configuration.MultiplierSettings[i].multiplier}x multiplier",
                    colour);
            Configuration.MultiplierSettings[i].colour = newColour;
        }

        ImGui.Spacing();

        ImGui.LabelText("##max_bet_lbl", "Max bet: ");
        ImGui.SameLine(80, 5);
        ImGui.SetNextItemWidth(105f);
        var iBuf = Configuration.MaxBet.ToString("N0");

        if (ImGui.InputText("##max_bet", ref iBuf, 64))
        {
            var num = iBuf.Replace(",", string.Empty).Replace(".", string.Empty);
            if (int.TryParse(num, out var parsedValue)) Configuration.MaxBet = parsedValue;
        }

        ImGui.Spacing();

        if (ImGui.Button("Save")) Configuration.Save();
    }

    private static int VerifyValue(int val)
    {
        val = val switch
        {
            < MinRoll => MinRoll,
            > MaxRoll => MaxRoll,
            _ => val
        };

        return val;
    }

    private void AddMultiplier()
    {
        Configuration.MultiplierSettings.Add(new DataStructures.MpSettings());
    }

    private void RemoveMultiplier(int i)
    {
        Configuration.MacroSettings.Remove(Configuration.MultiplierSettings[i].multiplier);
        Configuration.MultiplierSettings.Remove(Configuration.MultiplierSettings[i]);
    }
}
