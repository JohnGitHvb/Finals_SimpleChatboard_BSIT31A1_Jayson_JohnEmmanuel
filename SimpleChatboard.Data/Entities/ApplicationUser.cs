using Microsoft.AspNetCore.Identity;

namespace SimpleChatboard.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}