using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace HighRollerClassic;

public class Message(string? targetName, int? targetBet, string message) {
    private string message = message;
    private string? emote;

    private void GetEmote() {
        // multiple emotes
        foreach (var word in message.Split(" ")) {
            if (word.StartsWith('/')) {
                emote = word;
                break;
            }
        }
    }

    /// <summary>
    /// <para>We parse the message and swap the macros with the following:</para>
    /// #t - target<br />
    /// #b - target bet<br />
    /// </summary>
    private void ParseMessage() {
        // remove first command (startsWith /)
        if (emote != null) {
            message = message.Replace(emote, "");
        }

        // replace the known macros #t #b
        if (targetName != null)
            message = message.Replace("#t", targetName);
        if (targetBet != null)
            message = message.Replace("#b", targetBet.ToString());
    }

    /// <summary>
    /// Function sends the parsed message and emotes separately
    /// </summary>
    public void Send(bool yell, string? additionalMessage) {
        GetEmote(); // get emote must always be called first as this allows us to remove it from the message
        ParseMessage();

        if (additionalMessage != null) {
            message += $"\n{additionalMessage}";
        }

        SendMessage(false, yell, message); // sends message without the emote
        if (emote != null)
            SendMessage(true, false, emote); // sends emote
    }

    private static unsafe void SendMessage(bool isEmote, bool yell, string msg) {
        string messageToConvert;

        switch (isEmote) {
            case true:
            case false when !yell:
                messageToConvert = msg;
                break;
            case false:
                messageToConvert = $"/yell {msg}";
                break;
        }

        var mes = Utf8String.FromString(messageToConvert);
        UIModule.Instance()->ProcessChatBoxEntry(mes);
        mes->Dtor(true);
    }
}
