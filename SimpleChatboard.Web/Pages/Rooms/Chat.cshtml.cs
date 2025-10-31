using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Web.Data;
using SimpleChatboard.Web.Models;

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

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    public Room? Room { get; set; }
    public List<Message> Messages { get; set; } = new();
    public List<RoomUser> Members { get; set; } = new();
    [BindProperty]
    public string NewMessage { get; set; } = string.Empty;
    public List<Room> Rooms { get; set; } = new();
    public List<(string DisplayName, string Content, DateTime Timestamp)> DisplayMessages { get; set; } = new();
    public List<(string DisplayName, string UserId)> DisplayMembers { get; set; } = new();
    public bool IsCreator { get; set; }
    public string CreatorName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        Room = await _db.Rooms
            .Include(r => r.Users)
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == Id);
            
        if (Room == null) return NotFound();
        
        var userId = _userManager.GetUserId(User);
        IsCreator = Room.CreatedByUserId == userId;
        CreatorName = Room.CreatedBy?.DisplayName ?? Room.CreatedByUserId;
        
        Rooms = await _db.Rooms.AsNoTracking().ToListAsync();
        Members = await _db.RoomUsers
            .Include(ru => ru.User)
            .Where(ru => ru.RoomId == Id)
            .ToListAsync();
            
        Messages = await _db.Messages
            .Include(m => m.User)
            .Where(m => m.RoomId == Id)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        DisplayMessages = Messages
            .Select(m => (m.User?.DisplayName ?? m.UserId, m.Content, m.Timestamp))
            .ToList();
            
        DisplayMembers = Members
            .Select(m => (m.User?.DisplayName ?? m.UserId, m.UserId))
            .ToList();

        if (!Members.Any(m => m.UserId == userId))
        {
            if (Members.Count >= 50) return Forbid();
            
            _db.RoomUsers.Add(new RoomUser { RoomId = Id, UserId = userId! });
            await _db.SaveChangesAsync();
            Members.Add(new RoomUser { RoomId = Id, UserId = userId! });
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
        {
            return RedirectToPage(new { id = Id });
        }

        var userId = _userManager.GetUserId(User);
        var msg = new Message 
        { 
            RoomId = Id, 
            UserId = userId!, 
            Content = NewMessage, 
            Timestamp = DateTime.UtcNow 
        };
        
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteRoomAsync(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (room.CreatedByUserId != userId)
        {
            return Forbid();
        }

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        TempData["RoomDeleted"] = true;
        return RedirectToPage("/Index");
    }
}
