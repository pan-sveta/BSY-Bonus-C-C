using System.Text;
using System.Text.RegularExpressions;

namespace Pasture.Messages;

public class CommandMessage : IMessage<CommandMessage>
{
    public string Parameters { get; private set; }
    public bool Answered { get; set; }

    public CommandType Type { get; private set; }

    public CommandMessage(CommandType type, string parameters, bool answered = false)
    {
        Parameters = parameters;
        Type = type;
        Answered = answered;
    }

    public static bool TryParse(string message, out CommandMessage commandMessage)
    {
        Regex reg = new Regex("C€(?<type>\\d+)€(?<parameters>\\S*)€(?<answered>[y,n])");
        var match = reg.Match(message);

        if (!match.Success)
        {
            commandMessage = new CommandMessage(CommandType.W, "", false);
            return false;
        }

        commandMessage = new CommandMessage((CommandType)int.Parse(match.Groups["type"].Value),
            match.Groups["parameters"].Value, match.Groups["answered"].Value == "y");

        return true;
    }

    public string GetTransportFormat()
    {
        var sb = new StringBuilder();

        var answeredChar = Answered ? "y" : "n";
        var hidden = $"C€{(int)Type}€{Parameters}€{answeredChar}";

        sb.Append($"<!--");
        sb.Append(hidden);
        sb.Append("-->\n \n");

        sb.Append(Jokes.RandomJoke());

        return sb.ToString();
    }
}