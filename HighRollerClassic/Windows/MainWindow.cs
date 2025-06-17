using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Serilog;

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
        : base("High Roller Classic")
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

        // ImGui.TextUnformatted(Configuration.message);

        if (Configuration.multiplierSettings.Count == 0)
        {
            ImGui.Text("Please set up multipliers in settings first!");
            return;
        }

        var localPlayer = Plugin.ClientState.LocalPlayer;
        if (localPlayer == null) return;

        var tPlayer = Plugin.ClientState.LocalPlayer?.TargetObject;
        var targetIsPlayer = tPlayer is IPlayerCharacter;
        ImGui.TextUnformatted(
            $"TargetName: {(tPlayer != null && targetIsPlayer ? tPlayer.Name : "/")}");

        ImGui.SameLine();
        if (ImGui.Button("Greeting")) Greet();
        ImGui.SameLine();
        if (ImGui.Button("Settings")) Plugin.ToggleConfigUI();

        if (!targetIsPlayer || tPlayer == null) return;

        var pID = tPlayer.GameObjectId;

        if (!players.ContainsKey(pID)) players.Add(pID, new PlayerData());

        var inputBuffer = players[pID].bet.ToString("N0");
        ImGui.SetNextItemWidth(150f);
        if (ImGui.InputText("##player_bet", ref inputBuffer, 64))
        {
            var num = inputBuffer.Replace(",", string.Empty).Replace(".", string.Empty);
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

        ImGui.SameLine();
        if (ImGui.Button("clear"))
        {
            if (players.TryGetValue(pID, out var tempPlayer))
            {
                tempPlayer.rolls = new List<int>();
                tempPlayer.multipliers = new List<int>();
                tempPlayer.bets = new List<int>();
                players[pID] = tempPlayer;
            }
        }

        ImGui.Spacing();

        if (ImGui.BeginTable("rollHistoryTable", 3))
        {
            ImGui.TableSetupColumn("Roll");
            ImGui.TableSetupColumn("Multiplier");
            ImGui.TableSetupColumn("Bet");
            ImGui.TableHeadersRow();

            for (var i = 0; i < players[pID].bets.Count; i++)
            {
                ImGui.TableNextRow();

                var color = 0;
                foreach (var mp in Configuration.multiplierSettings)
                    if (mp[0] == players[pID].multipliers[i])
                    {
                        color = mp[3];
                        Log.Information("{ManifestName} Color {Color}\n", Plugin.PluginInterface.Manifest.Name, color);
                        break;
                    }

                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, (uint)color);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(players[pID].rolls[i].ToString());

                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{players[pID].multipliers[i].ToString()}x");

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(players[pID].bets[i].ToString("N0"));
            }

            ImGui.EndTable();
        }
    }

    private void Greet()
    {
        var greeting = $"{Configuration.greetingMessage}\n{GetRollsMultipliers()}";
        Plugin.SendMessage(greeting);
    }

    private string GetRollsMultipliers()
    {
        var message = "";

        // loop
        for (var i = 0; i < Configuration.multiplierSettings.Count; i++)
        {
            var mp = Configuration.multiplierSettings[i];
            var multiplier = mp[0];
            var roll = mp[1];
            var plus = mp[2] != (int)Comparators.Equal;
            var last = i == Configuration.multiplierSettings.Count - 1;

            message += $"[{roll}{(plus ? "+" : "")}: {multiplier}x]{(!last ? " O" : "")} ";
        }

        message += $"| MAX Bet: {Configuration.MaxBet:N0}";

        return message;
    }

    private void UpdatePlayerBet(ulong id, int bet)
    {
        if (players.ContainsKey(id))
        {
            var exData = players[id];
            exData.bet = bet;
            players[id] = exData;
        }
    }

    public void AddPlayerRoll(ulong id, int roll)
    {
        if (players.TryGetValue(id, out var exData))
        {
            var resManip = ManipulateBet(roll, exData.bet);
            exData.bet = resManip.bet;
            exData.rolls.Add(roll);
            exData.bets.Add(exData.bet);
            exData.multipliers.Add(resManip.multiplier);

            players[id] = exData;
        }
    }

    public (int bet, int multiplier) ManipulateBet(int roll, int bet)
    {
        foreach (var mp in Configuration.multiplierSettings)
            // if our comparator is equals (exact) and our roll matches given one from the player
            // multiply the bet with multiplier and return the value
            if (mp[2] == (int)Comparators.Equal && mp[1] == roll)
                return (bet * mp[0], mp[0]);

        // if not we go again
        foreach (var mp in Configuration.multiplierSettings.AsEnumerable().Reverse())
        {
            // if the comparator type is exact we skip the multiplier as we've done that above in previous loop
            // if the bet is bigger than the roll we set it to we multiply the bet with multiplier and return the value
            if (mp[2] == (int)Comparators.Equal) continue;

            if (roll >= mp[1]) return (bet * mp[0], mp[0]);
        }

        return (0, 0);
    }

    private struct PlayerData()
    {
        public int bet = 0;
        public List<int> rolls = new();
        public List<int> bets = new();
        public List<int> multipliers = new();
    }

    private enum Comparators
    {
        Equal = 0
    }
}
