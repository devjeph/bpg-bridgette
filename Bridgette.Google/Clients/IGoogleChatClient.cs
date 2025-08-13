namespace Bridgette.Google.Clients;

public interface IGoogleChatClient
{
    /// <summary>
    /// Sends a proactive direct message to a user.
    /// </summary>
    /// <param name="googleChatUserId">The user's Google Chat ID (e.g., "users/12345").</param>
    /// <param name="message">The text of the message to send.</param>
    Task SendDirectMessageAsync(string googleChatUserId, string message);

    /// <summary>
    /// Posts a reply message within an existing conversation thread.
    /// </summary>
    /// <param name="spaceName">The name of the space where the original message occurred.</param>
    /// <param name="threadName">The name of the thread to reply to.</param>
    /// <param name="message">The text of the reply message.</param>
    Task PostMessageInThreadAsync(string spaceName, string threadName, string message);
}