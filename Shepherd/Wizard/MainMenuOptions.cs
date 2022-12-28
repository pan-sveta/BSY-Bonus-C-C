using System.ComponentModel.DataAnnotations;

namespace Shepherd.Wizard;

public enum MainMenuOptions
{
    [Display(Name = "List sheep 🐑")]
    ListSheep,
    [Display(Name = "Control sheep 🕹️")]
    ControlSheep,
    [Display(Name = "Exit 🔚")]
    Exit
}