using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Data;

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