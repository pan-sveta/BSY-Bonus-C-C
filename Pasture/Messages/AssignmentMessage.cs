using System.Text;
using System.Text.RegularExpressions;

namespace Pasture.Messages;

public class AssignmentMessage : IMessage<AssignmentMessage>
{
    public string SheepId { get; private set; }
    public string? GistId { get; private set; }

    public AssignmentMessage(string sheepId, string? gistId)
    {
        SheepId = sheepId;
        GistId = gistId;
    }

    public static bool TryParse(string message, out AssignmentMessage assignmentMessage)
    {
        Regex reg = new Regex("(?<sheepId>\\w+)€(?<gistId>\\w+)");
        var match = reg.Match(message);

        if (!match.Success)
        {
            assignmentMessage = new AssignmentMessage("", "");
            return false;
        }

        assignmentMessage = new AssignmentMessage(match.Groups["sheepId"].Value, match.Groups["gistId"].Value);

        return true;
    }

    public string GetTransportFormat()
    {
        var hidden = $"{SheepId}€{GistId}";
        return Obfuscator.SheepLikeStatements(SheepId, hidden);
    }
}