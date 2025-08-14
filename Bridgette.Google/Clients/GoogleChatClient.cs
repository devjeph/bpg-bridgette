using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;

namespace Bridgette.Google.Clients;

public class GoogleChatClient : IGoogleChatClient
{
    private readonly ChatService _chatService;

    public GoogleChatClient(ChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessageAsync(string googleChatUserId, string message)
    {
        // To send a DM, we first need to open a DM space with the user.
        var dmRequest = new FindDirectMessageRequest { Name = googleChatUserId };
        var dmSpace = await _chatService.Spaces.FindDirectMessage(dmRequest).ExecuteAsync();
        
        var chatMessage = new Message { Text = message };
        await _chatService.Spaces.Messages.Create(chatMessage, dmSpace.Name).ExecuteAsync();
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