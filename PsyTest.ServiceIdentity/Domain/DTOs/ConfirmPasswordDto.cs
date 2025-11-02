namespace PsyTest.ServiceIdentity.Domain.DTOs
{
    public class ConfirmPasswordDto
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
