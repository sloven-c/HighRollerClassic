using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HighRollerClassic.Windows;

public class ConfigWindow : Window, IDisposable
{
    private const int MinRoll = 1, MaxRoll = 999;
    private readonly string[] comparators;
    private readonly Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(300, 300);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        comparators = ["=", ">="];
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        if (ImGui.Button("Add new...") || Configuration.MultiplierSettings.Count == 0) AddMultiplier();
        ImGui.Spacing();

        const float width = 50f;
        for (var i = 0; i < Configuration.MultiplierSettings.Count; i++)
        {
            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##multiplier{i}", ref Configuration.MultiplierSettings[i][0], 0);
            Configuration.MultiplierSettings[i][0] = VerifyValue(Configuration.MultiplierSettings[i][0]);


            ImGui.SameLine();

            ImGui.Text("x");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(width);
            ImGui.InputInt($"##roll{i}", ref Configuration.MultiplierSettings[i][1], 0);
            Configuration.MultiplierSettings[i][1] = VerifyValue(Configuration.MultiplierSettings[i][1]);

            ImGui.SameLine();

            ImGui.SetNextItemWidth(width * 1.1f);
            ImGui.Combo($"##comparators{i}", ref Configuration.MultiplierSettings[i][2], comparators,
                        comparators.Length);

            ImGui.SameLine();
            if (Configuration.MultiplierSettings.Count > 1)
            {
                if (ImGui.Button($"-##{i}"))
                    Configuration.MultiplierSettings.Remove(Configuration.MultiplierSettings[i]);
            }
        }

        ImGui.Spacing();

        ImGui.LabelText("##max_bet_lbl", "Max bet: ");
        ImGui.SameLine(80, 5);
        ImGui.SetNextItemWidth(105f);
        var iBuf = Configuration.MaxBet.ToString("N0");

        if (ImGui.InputText("##max_bet", ref iBuf, 64))
        {
            var num = iBuf.Replace(",", "");
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
        int[] newMultiplier = [0, 0, 1]; // multiplier, roll, comparator
        Configuration.MultiplierSettings.Add(newMultiplier);
    }
}
