using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleChatboard.Data.Entities;

namespace SimpleChatboard.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public ErrorModel(ILogger<ErrorModel> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public async Task OnGetAsync()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            _logger.LogError("Error for user {UserId} with RequestId {RequestId}", user.Id, RequestId);
        }
        else
        {
            _logger.LogError("Error for anonymous user with RequestId {RequestId}", RequestId);
        }
    }
}