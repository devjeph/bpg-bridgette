using Bridgette.Data.Models;

namespace Bridgette.Core.Services;

public interface IUserService
{
    /// <summary>
    /// Checks if a user is flagged as an administrator.
    /// </summary>
    /// <param name="googleChatUserId">The user's Google Chat ID.</param>
    /// <returns>True if the user is an admin, otherwise false.</returns>
    Task<bool> IsUserAdminAsync(string googleChatUserId);
    
    /// <summary>
    /// Handles the initial onboarding of a new user when they add the bot.
    /// Creates a disabled user record if one doesn't already exist.
    /// </summary>
    /// <param name="user">The user details from the Google Chat event.</param>
    /// <returns>The newly created or existing user.</returns>
    Task<User> OnboardUserAsync(User user);
    
    /// <summary>
    /// Updates a user's record to grant consent (sets IsEnabled = true).
    /// </summary>
    /// <param name="googleChatUserId">The user's Google Chat ID.</param>
    Task GrantConsentAsync(string googleChatUserId);
    
    /// <summary>
    /// Registers or updates a user's assigned Google Spreadsheet ID.
    /// </summary>
    /// <param name="googleChatUserId">The user's Google Chat ID.</param>
    /// <param name="spreadsheetId">The spreadsheet ID to assign.</param>
    Task RegisterTimesheetAsync(string googleChatUserId, string spreadsheetId);
}