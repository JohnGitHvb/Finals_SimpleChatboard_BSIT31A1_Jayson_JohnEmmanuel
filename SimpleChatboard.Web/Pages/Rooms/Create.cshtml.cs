using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

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
    public string Name { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);

        if (userId == null || user == null)
        {
            return Forbid();
        }

        var room = new Room
        {
            Name = Name,
            CreatedByUserId = userId,
            CreatedBy = user
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        // No need to explicitly add creator as member since Chat page will handle auto-joining

        return RedirectToPage("/Rooms/Chat", new { id = room.Id });
    }
}
