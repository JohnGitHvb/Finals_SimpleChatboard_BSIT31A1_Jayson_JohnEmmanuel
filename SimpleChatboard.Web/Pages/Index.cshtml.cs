using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Web.Data;

namespace SimpleChatboard.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Room> Rooms { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            Rooms = await _db.Rooms.AsNoTracking().ToListAsync();
        }
    }
}