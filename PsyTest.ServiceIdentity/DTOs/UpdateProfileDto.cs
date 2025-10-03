namespace PsyTest.ServiceIdentity.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; } // если нужно менять
        public string? Password { get; set; } // опционально, если юзер меняет пароль
    }
}
