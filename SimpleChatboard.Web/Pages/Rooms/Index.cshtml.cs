using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SimpleChatboard.Web.Models;
using Microsoft.AspNetCore.Identity;

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

    [BindProperty]
    public List<Room> Rooms { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Challenge();
        }

        ViewData["Title"] = "Rooms";
        Rooms = await _db.Rooms.AsNoTracking().ToListAsync();
        return Page();
    }
}
