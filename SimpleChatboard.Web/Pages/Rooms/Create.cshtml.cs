using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SimpleChatboard.Data.Entities;
using SimpleChatboard.Services;

namespace SimpleChatboard.Web.Pages.Rooms;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateModel(IRoomService roomService, UserManager<ApplicationUser> userManager)
    {
        _roomService = roomService;
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

        if (await _roomService.RoomExistsAsync(Room.Name))
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

        try
        {
            await _roomService.CreateRoomAsync(Room.Name, userId, user);
            TempData["RoomCreated"] = true;
            return RedirectToPage("Index");
        }
        catch
        {
            ErrorMessage = "An error occurred while creating the room. Please try again.";
            return Page();
        }
    }
}
