using System.Security.Cryptography;
using System.Text;

namespace Pasture;

public static class Obfuscator
{
    private static Random _rnd = new Random();

    private static string[] _thoseSheep = { "Those sheep", "Those pics", "Those pictures", "These"};
    private static string[] _positiveAdjectives = { "nice", "beautiful", "neat", "gorgeous", "cute", "lovely"};
    private static string[] _positiveAdjectives2nd = { "nicer", "more beautiful", "neater", "more gorgeous", "cuter", "more lovely"};
    
    public static string SheepLikeStatements(string seed, string hiddenContent)
    {
        var sb = new StringBuilder();
        sb.Append(_thoseSheep[_rnd.Next(_thoseSheep.Length)]);
        sb.Append(" are ");
        sb.Append(_positiveAdjectives[_rnd.Next(_positiveAdjectives.Length)]);
        sb.Append(" but this one is ");
        sb.Append(_positiveAdjectives2nd[_rnd.Next(_positiveAdjectives2nd.Length)]);
        sb.Append("!\n \n");

        sb.Append($"<img alt=\"{hiddenContent}\" src=\"https://picsum.photos/seed/${seed}/800/600\"/>");
        
        return sb.ToString();
}
}