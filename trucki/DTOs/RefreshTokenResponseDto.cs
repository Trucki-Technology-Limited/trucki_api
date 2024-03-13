namespace trucki.DTOs
{
    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string? UserId { get; set; }
        public string? EmailAddress { get; set; }
    }
}
