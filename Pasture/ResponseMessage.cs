using System.Text.RegularExpressions;

namespace Pasture;

public class ResponseMessage
{
    public int ForCommentId { get; private set; }
    public string Response { get; private set; }

    public ResponseMessage(int forCommentId, string response)
    {
        ForCommentId = forCommentId;
        Response = response;
    }

    public static bool TryParse(string message, out ResponseMessage responseMessage)
    {
        Regex reg = new Regex("R€(?<forCommentId>.+)€(?<response>.+)");
        var match = reg.Match(message);

        if (!match.Success)
        {
            responseMessage = new ResponseMessage(-1,"");
            return false;
        }
        
        responseMessage = new ResponseMessage(int.Parse(match.Groups["forCommentId"].Value),match.Groups["response"].Value );
        return true;
    }

    public string GetTransportFormat()
    {
        return $"R€{ForCommentId}€{Response}";
    }
}