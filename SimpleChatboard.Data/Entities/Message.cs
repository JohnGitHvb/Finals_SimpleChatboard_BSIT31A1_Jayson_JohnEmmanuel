using System;
using System.Collections.Generic;

namespace SimpleChatboard.Data.Entities;

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