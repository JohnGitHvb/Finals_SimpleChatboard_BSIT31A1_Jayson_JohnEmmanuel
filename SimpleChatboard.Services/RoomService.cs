using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Services;

public interface IRoomService
{
    Task<Room> CreateRoomAsync(string name, string userId, ApplicationUser user);
    Task<bool> RoomExistsAsync(string name);
}

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _db;

    public RoomService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> RoomExistsAsync(string name)
    {
        return await _db.Rooms.AnyAsync(r => r.Name == name);
    }

    public async Task<Room> CreateRoomAsync(string name, string userId, ApplicationUser user)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var room = new Room
            {
                Name = name,
                CreatedByUserId = userId,
                CreatedBy = user
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            var roomUser = new RoomUser
            {
                UserId = userId,
                RoomId = room.Id,
                User = user,
                Room = room
            };

            _db.RoomUsers.Add(roomUser);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return room;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}