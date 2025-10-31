using System;
using System.Collections.Generic;

namespace SimpleChatboard.Data.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<RoomUser> Users { get; set; } = new List<RoomUser>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}