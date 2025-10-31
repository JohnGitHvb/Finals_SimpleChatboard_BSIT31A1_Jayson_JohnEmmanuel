using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleChatboard.Web.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SimpleChatboard.Web.Models;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public Room Room { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Room.Name))
        {
            ErrorMessage = "Room name is required.";
            return Page();
        }

        if (await _db.Rooms.AnyAsync(r => r.Name == Room.Name))
        {
            ErrorMessage = "A room with this name already exists.";
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return Page();
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Set up the room
            Room.CreatedByUserId = userId;
            Room.CreatedBy = user;

            // Add the room
            _db.Rooms.Add(Room);
            await _db.SaveChangesAsync();

            // Create room membership
            var roomUser = new RoomUser
            {
                UserId = userId,
                RoomId = Room.Id,
                User = user,
                Room = Room
            };

            _db.RoomUsers.Add(roomUser);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["RoomCreated"] = true;
            return RedirectToPage("Index");
        }
        catch
        {
            await transaction.RollbackAsync();
            ErrorMessage = "An error occurred while creating the room. Please try again.";
            return Page();
        }
    }
}
