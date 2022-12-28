using System.Text;
using System.Text.RegularExpressions;

namespace Pasture.Messages;

public class ResponseMessage : IMessage<ResponseMessage>
{
    public int ForCommentId { get; private set; }
    public string Response { get; private set; }
    public CommandType Type { get; private set; }

    public ResponseMessage(int forCommentId, CommandType type, string response)
    {
        ForCommentId = forCommentId;
        Type = type;
        Response = response;
    }

    public static bool TryParse(string message, out ResponseMessage responseMessage)
    {
        Regex reg = new Regex("R€(?<type>\\d+)€(?<forCommentId>.+)€(?<response>[\\S\\s]*)-->");
        var match = reg.Match(message);

        if (!match.Success)
        {
            responseMessage = new ResponseMessage(-1, CommandType.W, "");
            return false;
        }

        responseMessage = new ResponseMessage(int.Parse(match.Groups["forCommentId"].Value),
            (CommandType)int.Parse(match.Groups["type"].Value), match.Groups["response"].Value);
        return true;
    }

    public string GetTransportFormat()
    {
        var sb = new StringBuilder();

        var hidden = $"R€{(int)Type}€{ForCommentId}€{Response}";

        sb.Append($"<!--");
        sb.Append(hidden);
        sb.Append("-->\n \n");

        sb.Append(Jokes.RandomJoke());

        return sb.ToString();
    }
}