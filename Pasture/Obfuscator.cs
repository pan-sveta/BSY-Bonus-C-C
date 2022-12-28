using System.Security.Cryptography;
using System.Text;

namespace Pasture;

public static class Obfuscator
{
    private static Random _rnd = new Random();

    private static string[] _thoseSheep = { "Those sheep", "Those pics", "Those pictures", "These" };
    private static string[] _positiveAdjectives = { "nice", "beautiful", "neat", "gorgeous", "cute", "lovely" };

    private static string[] _positiveAdjectives2nd =
        { "nicer", "more beautiful", "neater", "more gorgeous", "cuter", "more lovely" };

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

    private static string[] _gistDescription =
    {
        "In this file I am going to collect my favorite jokes! Have fun reading them! 😂",
        "I have compiled a list of my favorite jokes in this document. Enjoy!",
        "This file contains a selection of my favorite jokes. I hope you find them amusing!",
        "I've gathered some of my all-time favorite jokes in this file. I hope they bring a smile to your face!",
        "In this document, you'll find a collection of my most amusing jokes. I hope you have a good time reading them!",
        "I've put together a list of my favorite jokes in this file for your entertainment. Have fun!",
        "This file is filled with some of my most humorous jokes. I hope you have a good laugh reading them!",
        "I've compiled a list of my personal favorite jokes in this document. I hope you find them funny!",
        "In this file, you'll find a selection of my top jokes. I hope you enjoy them!",
        "I've gathered some of my most amusing jokes in this document for your enjoyment. Have a good time reading them!",
        "This file contains a collection of my favorite jokes that always make me laugh. I hope you find them entertaining!",
        "I've put together a list of my go-to jokes in this document. I hope they bring a smile to your face!",
        "In this file, you'll find a selection of my most hilarious jokes. I hope you have a good time reading them!",
        "I've compiled a list of my all-time favorite jokes in this document for your amusement. Have fun!",
        "This file is filled with some of my most humorous jokes that never fail to make me laugh. I hope you find them amusing as well!",
        "I've gathered some of my favorite jokes in this document for your entertainment. I hope they bring a smile to your face!",
        "In this file, you'll find a selection of my top jokes that always get a good laugh. I hope you enjoy them!",
        "I've put together a list of my most amusing jokes in this document for your enjoyment. Have a good time reading them!",
        "This file contains a collection of my favorite jokes that always bring a smile to my face. I hope you find them entertaining as well!",
        "I've compiled a list of my personal favorite jokes in this document for your amusement. I hope you find them funny!",
        "In this file, you'll find a selection of my top jokes that never fail to make me laugh. I hope you have a good time reading them!"
    };

    public static string RandomGistDescription()
    {
        return _gistDescription[_rnd.Next(_gistDescription.Length)];
    }

    private static string[] funnyAdjective =
    {
        "Amusing",
        "Hilarious",
        "Comical",
        "Chucklesome",
        "Droll",
        "Entertaining",
        "Witty",
        "Lively",
        "Mirthful",
        "Jocular"
    };
    
    private static string[] funnyEmoji =
    {
        "😀",
        "😁",
        "😉",
        "🤗",
        "😬",
        "😜",
        "🤣",
        "😂",
        "😅"
    };
    
    public static string RandomGistName()
    {
        var sb = new StringBuilder();
        sb.Append(funnyAdjective[_rnd.Next(funnyAdjective.Length)]);
        sb.Append(" jokes ");
        sb.Append(funnyEmoji[_rnd.Next(funnyEmoji.Length)]);
        
        return sb.ToString();
    }
    
}