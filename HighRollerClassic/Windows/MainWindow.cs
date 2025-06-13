using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HighRollerClassic.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly Dictionary<ulong, PlayerData> players;
    private readonly Plugin Plugin;
    private readonly int step;

    // fixme find a better way to do this shit
    // private int bet;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID",
               ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
        step = 500_000;
        Plugin = plugin;
        players = new Dictionary<ulong, PlayerData>();
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class

        if (Configuration.MultiplierSettings.Count == 0)
        {
            ImGui.Text("Please set up multipliers in settings first!");
            return;
        }

        var localPlayer = Plugin.ClientState.LocalPlayer;
        if (localPlayer == null) return;

        // ImGui.TextUnformatted($"PlayerName: {localPlayer.Name}");

        var tPlayer = Plugin.ClientState.LocalPlayer?.TargetObject;
        var targetIsPlayer = tPlayer is IPlayerCharacter;
        ImGui.TextUnformatted(
            $"TargetName: {(tPlayer != null && targetIsPlayer ? tPlayer.Name : "/")}");

        ImGui.SameLine();
        if (ImGui.Button("Settings")) Plugin.ToggleConfigUI();

        if (!targetIsPlayer || tPlayer == null) return;

        var pID = tPlayer.GameObjectId;

        if (!players.ContainsKey(pID)) players.Add(pID, new PlayerData());

        var inputBuffer = players[pID].bet.ToString("N0");
        if (ImGui.InputText("##player_bet", ref inputBuffer, 64))
        {
            var num = inputBuffer.Replace(",", "");
            if (int.TryParse(num, out var parsedValue)) UpdatePlayerBet(pID, parsedValue);
        }

        ImGui.SameLine();
        if (ImGui.Button("-"))
        {
            UpdatePlayerBet(pID, players[pID].bet - step);
            if (players[pID].bet < 0) UpdatePlayerBet(pID, 0);
        }

        ImGui.SameLine();
        if (ImGui.Button("+"))
        {
            UpdatePlayerBet(pID, players[pID].bet + step);
            if (players[pID].bet > Configuration.MaxBet)
                UpdatePlayerBet(pID, Configuration.MaxBet);
        }

        ImGui.SameLine();
        if (ImGui.Button("max")) UpdatePlayerBet(pID, Configuration.MaxBet);
    }

    private bool UpdatePlayerBet(ulong id, int bet)
    {
        if (players.ContainsKey(id))
        {
            var exData = players[id];
            exData.bet = bet;
            players[id] = exData;

            return true;
        }

        return false;
    }

    public bool AddPlayerRoll(ulong id, int roll)
    {
        if (players.TryGetValue(id, out var exData))
        {
            exData.rolls.Add(roll);
            players[id] = exData;

            return true;
        }

        return false;
    }

    private struct PlayerData()
    {
        public int bet = 0;
        public readonly List<int> rolls = new();
    }
}
