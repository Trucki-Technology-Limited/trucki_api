using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace trucki.DTOs
{
    public class ResetPasswordDto
    {
        public string? UserId { get; set; }
        public string? Token { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password must match")]
        public string ConfirmPassword { get; set; }
    }
}
