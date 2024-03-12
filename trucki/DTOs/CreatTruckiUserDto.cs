namespace trucki.DTOs
{
    public class CreatTruckiUserDto
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set;  }
        public List<string> Permissions { get; set; }
    }


    public class CreatTruckiUserResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public IEnumerable<string> Role { get; set; }
        public string EmailAddress { get; set; }
        public List<string> Permissions { get; set; }
        public bool? isPasswordChanged { get; set; }
        public bool? isEmailConfirmed { get; set; }
        public bool? isPhoneNumberConfirmed { get; set; }
    }       
}
