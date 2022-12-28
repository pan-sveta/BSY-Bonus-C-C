using System.Diagnostics;
using System.Text;
using CliWrap;

namespace Sheep.Core;

public static class CommandExecutor
{
    public static async Task<string> ExecuteCommand(string command)
    {
        var stdOut = new StringBuilder();

        //TODO: Add /bin
        var result = await Cli.Wrap(@"/bin/bash")
            .WithArguments($" -c \"{command}\"")
                .WithValidation(CommandResultValidation.None)
                //.WithStandardInputPipe(PipeSource.FromCommand(lsCommand))
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdOut))
                .ExecuteAsync();

        return stdOut.ToString();
    }
    
    public static async Task<string> ExecuteCommandW()
    {
        return await ExecuteCommand($"w");
    }
    
    public static async Task<string> ExecuteCommandLs(string path)
    {
        return await ExecuteCommand($"ls {path}");
    }
    
    public static async Task<string> ExecuteCommandId()
    {
        return await ExecuteCommand($"id");
    }
    
    public static async Task<string> ExecuteCommandCopy(string path)
    {
        return await ExecuteCommand($"cat {path}");
    }
}