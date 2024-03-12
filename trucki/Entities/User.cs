using Microsoft.AspNetCore.Identity;

namespace trucki.Entities;

public class User: IdentityUser
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public bool IsActive { get; set; }
    public bool IsPasswordChanged { get; set; } = false;
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime? LastLogin { get; set; } 
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
}