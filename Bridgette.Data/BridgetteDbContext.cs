using Bridgette.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bridgette.Data;

public class BridgetteDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public BridgetteDbContext(DbContextOptions<BridgetteDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.GoogleChatUserId).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.GoogleChatUserId).IsUnique();
            entity.Property(u => u.UserDisplayName).IsRequired().HasMaxLength(255);
            entity.Property(u => u.UserEmail).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.UserEmail).IsUnique();
            entity.Property(u => u.AssignedSpreadsheetId).IsRequired(false).HasMaxLength(255);
            entity.Property(u => u.IsEnabled).IsRequired().HasDefaultValue(false);
            entity.Property(u => u.IsAdmin).IsRequired().HasDefaultValue(false);
            entity.Property(u => u.LastNotifiedTimestamp).IsRequired(false);
            entity.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        });
    }
}