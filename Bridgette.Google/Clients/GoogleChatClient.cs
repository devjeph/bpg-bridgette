using Bridgette.Google.Dtos;
using Google.Apis.Auth.OAuth2;
using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Thread = Google.Apis.HangoutsChat.v1.Data.Thread;

namespace Bridgette.Google.Clients;

public class GoogleChatClient : IGoogleChatClient
{
    private readonly HangoutsChatService _chatService;

    public GoogleChatClient(IOptions<GoogleApiSettings> googleApiSettings)
    {
        var credential = GoogleCredential.FromJson(googleApiSettings.Value.ServiceAccountKeyJson)
            .CreateScoped(HangoutsChatService.Scope.ChatBot);

        _chatService = new HangoutsChatService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "BPG Bridgette",
        });
    }

    public async Task SendDirectMessageAsync(string googleChatUserId, string message)
    {
        // Note: The FindDirectMessageRequest is not part of the library itself.
        // We must create the DM space by sending a message to a special endpoint.
        var dmSpace = new Space
        {
            Name = $"users/{googleChatUserId.Split('/').Last()}",
        };

        var chatMessage = new Message
        {
            Text = message,
        };
        
        // For sending a DM, the "parent" is the DM space itself.
        await _chatService.Spaces.Messages.Create(chatMessage, $"spaces/{dmSpace.Name}").ExecuteAsync();
    }

    public async Task PostMessageInThreadAsync(string spaceName, string threadName, string message)
    {
        var chatMessage = new Message
        {
            Text = message,
            Thread = new Thread { Name = threadName }
        };

        await _chatService.Spaces.Messages.Create(chatMessage, spaceName).ExecuteAsync();
    }
}