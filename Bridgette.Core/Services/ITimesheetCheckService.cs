namespace Bridgette.Core.Services;

public interface ITimesheetCheckService
{
    /// <summary>
    /// Executes a timesheet check for a single user on demand.
    /// </summary>
    /// <param name="googleChatUserId">The user's Google Chat ID.</param>
    /// <returns>A status message indicating the result of the check.</returns>\
    Task<string> ExecuteOnDemandCheckAsync(string googleChatUserId);
    
    /// <summary>
    /// Initiates a bulk timesheet check for all enabled users for a specific month.
    /// </summary>
    /// <param name="month">The month to check in YYYYMM format.</param>
    /// <returns>A summary message for the admin who initiated the check.</returns>
    Task<string> ExecuteBulkCheckAsync(string month);
}