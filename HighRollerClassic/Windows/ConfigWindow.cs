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
        if (ImGui.Button("Add new...") || Configuration.multiplierSettings.Count == 0) AddMultiplier();
        ImGui.SameLine();
        if (ImGui.Button("Macro settings")) Plugin.ToggleMacroUI();
        ImGui.Spacing();

        const float width = 50f;
        for (var i = 0; i < Configuration.multiplierSettings.Count; i++)
        {
            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##multiplier{i}", ref Configuration.multiplierSettings[i][0], 0);
            Configuration.multiplierSettings[i][0] =
                VerifyValue(Configuration.multiplierSettings[i][0]);

            ImGui.SameLine();

            ImGui.Text("x");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##roll{i}", ref Configuration.multiplierSettings[i][1], 0);
            Configuration.multiplierSettings[i][1] = VerifyValue(Configuration.multiplierSettings[i][1]);

            ImGui.SameLine();

            ImGui.SetNextItemWidth(width * 1.1f);
            ImGui.Combo($"##comparators{i}", ref Configuration.multiplierSettings[i][2], comparators,
                        comparators.Length);

            ImGui.SameLine();
            if (Configuration.multiplierSettings.Count > 1)
            {
                if (ImGui.Button($"-##{i}"))
                    RemoveMultiplier(i);
            }

            ImGui.SameLine();
            var color = ImGui.ColorConvertU32ToFloat4((uint)Configuration.multiplierSettings[i][3]);
            var newColor =
                ImGuiComponents.ColorPickerWithPalette(i, $"{Configuration.multiplierSettings[i][0]}x multiplier",
                                                       color);
            Configuration.multiplierSettings[i][3] = (int)ImGui.ColorConvertFloat4ToU32(newColor);
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
        int[] newMultiplier = [0, 0, 1, 0]; //multiplier, roll, comparator, color
        Configuration.multiplierSettings.Add(newMultiplier);
    }

    private void RemoveMultiplier(int i)
    {
        Configuration.multiplierMessages.Remove(Configuration.multiplierSettings[i][0]);
        Configuration.multiplierSettings.Remove(Configuration.multiplierSettings[i]);
    }
}
