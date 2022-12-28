using System.Text;
using System.Text.RegularExpressions;

namespace Pasture.Messages;

public class HeartBeatMessage : IMessage<HeartBeatMessage>
{
    public DateTimeOffset Timestamp { get; set; }

    public HeartBeatMessage(DateTimeOffset timestamp)
    {
        Timestamp = timestamp;
    }

    public static bool TryParse(string message, out HeartBeatMessage heartBeatMessage)
    {
        Regex reg = new Regex("H€(?<timestamp>\\d+)");
        var match = reg.Match(message);

        if (!match.Success)
        {
            heartBeatMessage = new HeartBeatMessage(DateTimeOffset.MinValue);
            return false;
        }

        var epochTime = int.Parse(match.Groups["timestamp"].Value);
        heartBeatMessage = new HeartBeatMessage(DateTimeOffset.FromUnixTimeSeconds(epochTime));

        return true;
    }

    public string GetTransportFormat()
    {
        var sb = new StringBuilder();

        var hidden = $"H€{Timestamp.ToUnixTimeSeconds()}";

        sb.Append($"<!--");
        sb.Append(hidden);
        sb.Append("-->\n \n");

        sb.Append(Jokes.RandomJoke());

        return sb.ToString();
    }
}