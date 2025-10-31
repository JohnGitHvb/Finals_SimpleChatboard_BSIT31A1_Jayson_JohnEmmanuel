using System;

namespace SimpleChatboard.Data.Entities;

public class RoomUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Room Room { get; set; } = null!;
    public int RoomId { get; set; }
    public ApplicationUser? User { get; set; }
}