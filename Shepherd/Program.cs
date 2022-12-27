// See https://aka.ms/new-console-template for more information

using System.Text;
using Octokit;
using Shepherd.Core;
using Shepherd.Wizard;
using Sharprompt;

var shepherdController = new ShepherdController();

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"Welcome my dearest sheep shepherd! What can I do for you?");

var wizard = new Wizard(shepherdController);
await wizard.MainMenu();

