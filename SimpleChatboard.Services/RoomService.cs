using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Services;

public interface IRoomService
{
    Task<Room> CreateRoomAsync(string name, string userId, ApplicationUser user);
    Task<bool> RoomExistsAsync(string name);
    Task<List<Room>> GetUserRoomsAsync(string userId);
    Task<Room?> GetRoomByIdAsync(int id);
    Task<Room?> GetRoomWithDetailsAsync(int id);
    Task DeleteRoomAsync(int id, string userId);
    Task<bool> IsUserInRoomAsync(int roomId, string userId);
    Task AddUserToRoomAsync(int roomId, string userId, ApplicationUser user);
    Task RemoveUserFromRoomAsync(int roomId, string userId);
    Task<Message> AddMessageAsync(int roomId, string userId, string content, ApplicationUser user);
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

    public async Task<List<Room>> GetUserRoomsAsync(string userId)
    {
        return await _db.Rooms
            .Include(r => r.Users)
            .Where(r => r.Users.Any(u => u.UserId == userId))
            .OrderByDescending(r => r.Id)
            .ToListAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _db.Rooms
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetRoomWithDetailsAsync(int id)
    {
        return await _db.Rooms
            .Include(r => r.Messages)
            .ThenInclude(m => m.User)
            .Include(r => r.Users)
            .ThenInclude(ru => ru.User)
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task DeleteRoomAsync(int id, string userId)
    {
        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == id && r.CreatedByUserId == userId);

        if (room != null)
        {
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserInRoomAsync(int roomId, string userId)
    {
        return await _db.RoomUsers
            .AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);
    }

    public async Task AddUserToRoomAsync(int roomId, string userId, ApplicationUser user)
    {
        if (await IsUserInRoomAsync(roomId, userId))
            return;

        var room = await _db.Rooms.FindAsync(roomId);
        if (room == null)
            throw new ArgumentException("Room not found", nameof(roomId));

        var roomUser = new RoomUser
        {
            UserId = userId,
            RoomId = roomId,
            User = user,
            Room = room
        };

        _db.RoomUsers.Add(roomUser);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveUserFromRoomAsync(int roomId, string userId)
    {
        var roomUser = await _db.RoomUsers
            .FirstOrDefaultAsync(ru => ru.RoomId == roomId && ru.UserId == userId);

        if (roomUser != null)
        {
            _db.RoomUsers.Remove(roomUser);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<Message> AddMessageAsync(int roomId, string userId, string content, ApplicationUser user)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room == null)
            throw new ArgumentException("Room not found", nameof(roomId));

        if (!await IsUserInRoomAsync(roomId, userId))
            throw new InvalidOperationException("User is not a member of this room");

        var message = new Message
        {
            Content = content,
            RoomId = roomId,
            UserId = userId,
            User = user,
            Room = room,
            Timestamp = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return message;
    }
}