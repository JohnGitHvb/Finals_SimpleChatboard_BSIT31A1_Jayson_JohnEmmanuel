using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleChatboard.Web.Data;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class MembersModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public MembersModel(ApplicationDbContext db) => _db = db;

    public int RoomId { get; set; }
    public List<(string DisplayName, string UserId)> DisplayMembers { get; set; } = new();

    public async Task OnGetAsync(int id)
    {
        RoomId = id;
        var members = await _db.RoomUsers
            .Include(ru => ru.User)
            .Where(ru => ru.RoomId == id)
            .ToListAsync();
            
        DisplayMembers = members
            .Select(m => (m.User?.DisplayName ?? m.UserId, m.UserId))
            .ToList();
    }
}
