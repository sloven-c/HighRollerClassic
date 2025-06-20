using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HighRollerClassic.Windows;

public class MacroWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly uint maxLen;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public MacroWindow(Plugin plugin) : base("HRC Macro Settings")
    {
        Flags = ImGuiWindowFlags.NoCollapse;

        Size = new Vector2(500, 200);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        maxLen = 8192;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        foreach (var mp in Configuration.MultiplierSettings)
            Configuration.MultiplierMessages.TryAdd(mp[0], ("", new Configuration.MacroCheckbox()));
    }

    public override void Draw()
    {
        DrawCheckboxes(ref Configuration.Greeting.checkbox, "greeting");
        ImGui.TextUnformatted("Greeting message: ");
        ImGui.SameLine();
        ImGui.InputText("##greeting", ref Configuration.Greeting.message, maxLen);

        // get all multipliers
        foreach (var message in Configuration.MultiplierMessages)
        {
            var msg = message.Value;
            DrawCheckboxes(ref msg.checkbox, message.Key.ToString());
            ImGui.TextUnformatted($"{message.Key}x multiplier message: ");
            ImGui.SameLine();


            if (ImGui.InputText($"##multiplerMSG{message.Key}", ref msg.message, maxLen))
                Configuration.MultiplierMessages[message.Key] = msg;
        }

        DrawCheckboxes(ref Configuration.Lose.checkbox, "loser");
        ImGui.TextUnformatted("Lose message: ");
        ImGui.SameLine();
        ImGui.InputText("##loser", ref Configuration.Lose.message, maxLen);

        ImGui.Spacing();
        if (ImGui.Button("Save")) Configuration.Save();
    }

    private void DrawCheckboxes(ref Configuration.MacroCheckbox mChk, string id)
    {
        ImGui.Checkbox($"##Preview_{id}", ref mChk.Preview);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Preview");
        ImGui.SameLine();
        ImGui.Checkbox($"##yell_{id}", ref mChk.Yell);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Yell");
        ImGui.SameLine();
    }
}
