using Bridgette.Data;
using Bridgette.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bridgette.Core.Services;

public class UserService : IUserService
{
    private readonly BridgetteDbContext _dbContext;

    public UserService(BridgetteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsUserAdminAsync(string googleChatUserId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.GoogleChatUserId == googleChatUserId);
        
        return user is { IsAdmin: true, IsEnabled: true };
    }

    public async Task<User> OnboardUserAsync(User user)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleChatUserId == user.GoogleChatUserId);

        if (existingUser != null)
        {
            return existingUser;
        }
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task GrantConsentAsync(string googleChatUserId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleChatUserId == googleChatUserId);

        if (user != null)
        {
            user.IsEnabled = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RegisterTimesheetAsync(string googleChatUserId, string spreadsheetId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleChatUserId == googleChatUserId);

        if (user != null)
        {
            user.AssignedSpreadsheetId = spreadsheetId;
            await _dbContext.SaveChangesAsync();
        }
    }
}