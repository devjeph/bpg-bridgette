using Bridgette.Data;
using Bridgette.Google.Clients;
using Microsoft.EntityFrameworkCore;

namespace Bridgette.Core.Services;

public class TimesheetCheckService : ITimesheetCheckService
{
    private readonly BridgetteDbContext _dbContext;
    private readonly IGoogleSheetsClients _sheetsClient;
    private readonly IGoogleChatClient _chatClient;

    public TimesheetCheckService(BridgetteDbContext dbContext, IGoogleSheetsClients sheetsClient,
        IGoogleChatClient chatClient)
    {
        _dbContext = dbContext;
        _sheetsClient = sheetsClient;
        _chatClient = chatClient;
    }

    public async Task<string> ExecuteOnDemandCheckAsync(string googleChatUserId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.GoogleChatUserId == googleChatUserId && u.IsEnabled);

        if (user == null) return "User not found or has not consented.";
        if (string.IsNullOrEmpty(user.AssignedSpreadsheetId))
            return "You have not registered a timesheet. User /registertimesheet <ID>.";
        
        var month = DateTime.UtcNow.ToString("yyyyMM");
        bool isPopulated = await _sheetsClient.IsSheetPopulatedAsync(user.AssignedSpreadsheetId, month);

        return isPopulated
            ? $"Your timesheet for {month} looks good!"
            : $"Your timesheet for {month} appears to be empty.";
    }

    public async Task<string> ExecuteBulkCheckAsync(string month)
    {
        var usersToCheck = await _dbContext.Users
            .Where(u => u.IsEnabled && u.AssignedSpreadsheetId != null)
            .ToListAsync();

        foreach (var user in usersToCheck)
        {
            bool isPopulated = await _sheetsClient.IsSheetPopulatedAsync(user.AssignedSpreadsheetId, month);
            if (!isPopulated)
            {
                var message =
                    $"Hi {user.UserDisplayName}, this is a friendly reminder to please fill out your timesheet for {month}.";
                await _chatClient.SendDirectMessageAsync(user.GoogleChatUserId, message);
                
                user.LastNotifiedTimestamp = DateTime.UtcNow;
            }
        }
        
        await _dbContext.SaveChangesAsync();
        return $"Bulk check initiated for {month}. Reminders will be sent to users with incomplete timesheets.";
    }
}