using System.Text.RegularExpressions;

namespace Pasture;

public class CommandMessage
{
    public string Command { get; private set; }
    public bool Answered { get; set; }

    public CommandMessage(string command, bool answered)
    {
        Command = command;
        Answered = answered;
    }

    public static bool TryParse(string message, out CommandMessage commandMessage)
    {
        Regex reg = new Regex("C€(?<command>.+)€(?<answered>[y,n])");
        var match = reg.Match(message);
        
        if (!match.Success)
        {
            commandMessage = new CommandMessage("",false);
            return false;
        }
        
        commandMessage = new CommandMessage(match.Groups["command"].Value,match.Groups["answered"].Value == "y");
        return true;
    }

    public string GetTransportFormat()
    {
        string answeredChar = Answered ? "y" : "n";
        return $"C€{Command}€{answeredChar}";
    }
}