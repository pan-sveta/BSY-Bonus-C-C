// See https://aka.ms/new-console-template for more information

using System.Text;
using Octokit;
using Shepherd.Core;
using Shepherd.Wizard;
using Sharprompt;

Console.ForegroundColor = ConsoleColor.Blue;
Console.Write(@"   _____ _                _                  _ 
  / ____| |              | |                | |
 | (___ | |__   ___ _ __ | |__   ___ _ __ __| |
  \___ \| '_ \ / _ \ '_ \| '_ \ / _ \ '__/ _` |
  ____) | | | |  __/ |_) | | | |  __/ | | (_| |
 |_____/|_| |_|\___| .__/|_| |_|\___|_|  \__,_|
                   | |                         
                   |_|                         ");

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine(@"
        ____    ,-.
       /   /)))( , )
      (  c'a(   \`'
      _) ) _/   |
      \_/ (_    |
      / \`~\\   |
     (,,,)  )) _j
      | /''((_/(_]
      \ \   `./ |
     ,'\ \_   `.|
    /   `._\    \
   /,,,      ,,,,\         _.-..
  /__|=     =\__\=\      ,'9 )\)`-.,.--.
 /'''',,,,   '```  \     `-.|           `.
/    =|_|=          \       \,      ,    \)
 `-._ '```     ___.-'        `.  )._\   (\
  |(/`--....--'\ \             |//   `-,//");           
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(@"v,,,vVvVvv,VVVvv,v,Vw`-. \vv  BSY  Filip Stepanek  vVv,,vVvv
    ,,vhjWvv`-`
");
Console.ForegroundColor = ConsoleColor.Gray;

var shepherdController = new ShepherdController();

Console.OutputEncoding = Encoding.UTF8;

var wizard = new Wizard(shepherdController);
await wizard.MainMenu();

