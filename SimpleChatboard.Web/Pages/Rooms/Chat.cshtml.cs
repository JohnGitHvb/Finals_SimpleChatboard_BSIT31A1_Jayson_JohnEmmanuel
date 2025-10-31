using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class ChatModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Room? Room { get; set; }
    public IList<Room> Rooms { get; set; } = new List<Room>();
    public IList<Message> Messages { get; set; } = new List<Message>();
    public IList<RoomUser> Users { get; set; } = new List<RoomUser>();
    public string? ErrorMessage { get; set; }
    
    [BindProperty]
    public string NewMessage { get; set; } = string.Empty;
    
    public bool IsCreator => Room?.CreatedByUserId == _userManager.GetUserId(User);
    public string CreatorName => Room?.CreatedBy?.DisplayName ?? Room?.CreatedBy?.UserName ?? "Unknown";
    public IList<RoomUser> Members => Users;
    public IList<(string DisplayName, DateTime Timestamp, string Content)> DisplayMessages =>
        Messages.Select(m => (
            DisplayName: m.User?.DisplayName ?? m.User?.UserName ?? "Unknown",
            m.Timestamp,
            m.Content
        )).ToList();
    public IList<(string DisplayName, string UserId)> DisplayMembers =>
        Users.Select(u => (
            DisplayName: u.User?.DisplayName ?? u.User?.UserName ?? "Unknown",
            UserId: u.UserId
        )).ToList();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Forbid();
        }

        // Get all rooms for the left panel
        Rooms = await _db.Rooms
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        Room = await _db.Rooms
            .Include(r => r.Messages)
            .ThenInclude(m => m.User)
            .Include(r => r.Users)
            .ThenInclude(ru => ru.User)
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (Room == null)
        {
            ErrorMessage = "Room not found.";
            return RedirectToPage("/Index");
        }

        // Always ensure user is a member of the room
        if (!Room.Users.Any(u => u.UserId == userId))
        {
            var user = await _userManager.GetUserAsync(User);
            var roomUser = new RoomUser
            {
                RoomId = Room.Id,
                UserId = userId,
                Room = Room,
                User = user
            };
            
            Room.Users.Add(roomUser);
            await _db.SaveChangesAsync();
        }

        Messages = Room.Messages.OrderByDescending(m => m.Timestamp).ToList();
        Users = Room.Users.ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
        {
            return RedirectToPage(new { id });
        }

        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);
        if (userId == null || user == null)
        {
            return Forbid();
        }

        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            ErrorMessage = "Room not found.";
            return RedirectToPage("/Index");
        }

        var message = new Message
        {
            Content = NewMessage,
            RoomId = id,
            UserId = userId,
            User = user,
            Room = room,
            Timestamp = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return RedirectToPage(new { id });
    }
    
    public async Task<IActionResult> OnPostDeleteRoomAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Forbid();
        }

        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == id && r.CreatedByUserId == userId);

        if (room == null)
        {
            return NotFound();
        }

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}
