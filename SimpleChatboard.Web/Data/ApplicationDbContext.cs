using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Web.Models;

namespace SimpleChatboard.Web.Data;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<RoomUser> Users { get; set; } = new List<RoomUser>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class RoomUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Room Room { get; set; } = null!;
    public int RoomId { get; set; }
    public ApplicationUser? User { get; set; }
}

public class Message
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Room Room { get; set; } = null!;
    public int RoomId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ApplicationUser? User { get; set; }
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomUser> RoomUsers => Set<RoomUser>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<RoomUser>()
            .HasIndex(ru => new { ru.UserId, ru.RoomId })
            .IsUnique();
            
        builder.Entity<Room>()
            .HasMany(r => r.Users)
            .WithOne(ru => ru.Room)
            .HasForeignKey(ru => ru.RoomId);
            
        builder.Entity<Room>()
            .HasMany(r => r.Messages)
            .WithOne(m => m.Room)
            .HasForeignKey(m => m.RoomId);

        builder.Entity<Room>()
            .HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .IsRequired(false);

        builder.Entity<RoomUser>()
            .HasOne(ru => ru.User)
            .WithMany()
            .HasForeignKey(ru => ru.UserId)
            .IsRequired();

        builder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .IsRequired();
    }
}
