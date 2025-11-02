namespace PsyTest.ServiceIdentity.Domain.DTOs
{
    public record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword,
        string ConfirmUrl
    );
}
