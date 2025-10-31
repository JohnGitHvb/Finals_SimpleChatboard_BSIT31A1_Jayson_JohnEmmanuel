using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleChatboard.Data.Entities;
using SimpleChatboard.Services;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IRoomService roomService, UserManager<ApplicationUser> userManager)
    {
        _roomService = roomService;
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

        Rooms = await _roomService.GetUserRoomsAsync(userId);

        if (TempData["RoomCreated"] is true)
        {
            StatusMessage = "Room created successfully!";
            TempData.Remove("RoomCreated");
        }
        else if (TempData["RoomDeleted"] is true)
        {
            StatusMessage = "Room deleted successfully!";
            TempData.Remove("RoomDeleted");
        }

        return Page();
    }
}
