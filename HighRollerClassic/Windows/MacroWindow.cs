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
        foreach (var mp in Configuration.multiplierSettings) Configuration.multiplierMessages.TryAdd(mp[0], "");
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Greeting message: ");
        ImGui.SameLine();
        ImGui.InputText("##greeting", ref Configuration.greetingMessage, maxLen);

        // get all multipliers
        foreach (var message in Configuration.multiplierMessages)
        {
            ImGui.TextUnformatted($"{message.Key}x multiplier message: ");
            ImGui.SameLine();

            var msg = message.Value;
            if (ImGui.InputText($"##multiplerMSG{message.Key}", ref msg, maxLen))
                Configuration.multiplierMessages[message.Key] = msg;
        }

        ImGui.TextUnformatted("Lose message: ");
        ImGui.SameLine();
        ImGui.InputText("##loser", ref Configuration.loseMessage, maxLen);

        ImGui.Spacing();
        if (ImGui.Button("Save")) Configuration.Save();
    }
}
