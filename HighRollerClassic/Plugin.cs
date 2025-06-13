using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HighRollerClassic.Windows;

namespace HighRollerClassic;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/hrc";

    public readonly WindowSystem WindowSystem = new("HighRollerClassic");

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        SettingsWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(SettingsWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the interface for High Roller Classic game."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [HighRollerClassic] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");

        ChatGui.ChatMessageUnhandled += ChatMessage;
    }

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    private ConfigWindow SettingsWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public void Dispose()
    {
        ChatGui.ChatMessageUnhandled -= ChatMessage;
        WindowSystem.RemoveAllWindows();

        SettingsWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void ChatMessage(XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (type != (XivChatType)8266) return;

        // get targetted players
        var tObject = ClientState.LocalPlayer?.TargetObject;
        if (tObject == null) return;

        var tPlayer = tObject.Name.ToString().Split(" ");

        // get name and value from message
        var cleanedMessage = message.ToString().Replace(".", string.Empty).Split(" ");
        var validGambler = false;

        for (var i = 0; i < cleanedMessage.Length; i++)
            if (i + 1 < cleanedMessage.Length && cleanedMessage[i] == tPlayer[0] &&
                cleanedMessage[i + 1] == tPlayer[1])
            {
                validGambler = true;
                break;
            }

        if (!validGambler) return;

        if (int.TryParse(cleanedMessage[^1], out var result)) MainWindow.AddPlayerRoll(tObject.GameObjectId, result);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void ToggleConfigUI()
    {
        SettingsWindow.Toggle();
    }

    public void ToggleMainUI()
    {
        MainWindow.Toggle();
    }
}
