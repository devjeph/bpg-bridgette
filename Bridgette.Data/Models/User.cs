namespace Bridgette.Data.Models;

public class User
{
    public int Id { get; set; }
    public required string GoogleChatUserId { get; set; }
    public required string UserDisplayName { get; set; }
    public required string UserEmail { get; set; }
    public string? AssignedSpreadsheetId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime? LastNotifiedTimestamp { get; set; }
    public DateTime CreatedAt { get; set; }
}