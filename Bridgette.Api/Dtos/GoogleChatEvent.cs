using System.Text.Json.Serialization;

namespace Bridgette.Api.Dtos;

/// <summary>
/// Represents the overall event payload from Google Chat
/// </summary>
public class GoogleChatEvent
{
    [JsonPropertyName("type")] 
    public string Type { get; set; }
    
    [JsonPropertyName("user")]
    public GoogleChatUser User { get; set; }
    
    [JsonPropertyName("space")]
    public GoogleChatSpace Space { get; set; }
    
    [JsonPropertyName("message")]
    public GoogleChatMessage? Message { get; set; }
    
    [JsonPropertyName("common")]
    public GoogleChatCardCommon? Common { get; set; }
    
}

public class GoogleChatUser
{
    [JsonPropertyName("name")] 
    public string Name { get; set; }
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class GoogleChatSpace
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class GoogleChatMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("thread")]
    public GoogleChatThread Thread { get; set; }
    
    [JsonPropertyName("slashCommand")]
    public GoogleChatSlashCommand? SlashCommand { get; set; }
}

public class GoogleChatCardCommon
{
    [JsonPropertyName("invokedFunction")]
    public string InvokedFunction { get; set; }
}

public class GoogleChatThread
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class GoogleChatSlashCommand
{
    [JsonPropertyName("commandId")]
    public string CommandId { get; set; }
}