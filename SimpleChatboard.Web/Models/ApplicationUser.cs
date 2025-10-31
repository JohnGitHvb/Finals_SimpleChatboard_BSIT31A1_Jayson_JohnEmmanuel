using Microsoft.AspNetCore.Identity;

namespace SimpleChatboard.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}