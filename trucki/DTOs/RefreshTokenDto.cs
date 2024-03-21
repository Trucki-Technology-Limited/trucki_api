using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace trucki.DTOs
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token required")]
        public string RefreshToken { get; set; }
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }
    }
}
