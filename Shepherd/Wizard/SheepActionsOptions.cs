using System.ComponentModel.DataAnnotations;

namespace Shepherd.Wizard;

public enum SheepActionsOptions
{
    [Display(Name = "w - list of users currently logged in")]
    W,
    [Display(Name = "ls <PATH> - content of specified directory")]
    Ls,
    [Display(Name = "id - if of current user")]
    Id,
    [Display(Name = "cp <PATH> - copies remote file to your device")]
    Copy,
    [Display(Name = "<PATH> - executes binary on remote device")]
    Execute,
    Return
}