using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public IList<Room> Rooms { get; set; } = new List<Room>();
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Forbid();
        }

        Rooms = await _db.Rooms
            .Where(r => r.Users.Any(u => u.UserId == userId))
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        if (TempData["RoomCreated"] is true)
        {
            StatusMessage = "Room created successfully!";
            TempData.Remove("RoomCreated");
        }

        return Page();
    }
}
