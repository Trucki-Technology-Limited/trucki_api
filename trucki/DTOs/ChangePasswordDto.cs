using System.ComponentModel.DataAnnotations;

namespace trucki.DTOs
{
    public class ChangePasswordDto
    {
        public string? userId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}

