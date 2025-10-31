using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Data;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public IList<Room> Rooms { get; set; } = new List<Room>();

    public async Task OnGetAsync()
    {
        Rooms = await _db.Rooms.OrderByDescending(r => r.Id).Take(10).ToListAsync();
    }
}