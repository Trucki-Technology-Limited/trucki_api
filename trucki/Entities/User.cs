using Microsoft.AspNetCore.Identity;

namespace trucki.Entities;

public class User: IdentityUser
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
}