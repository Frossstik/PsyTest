using MediatR;
using PsyTest.ServiceIdentity.Common;

namespace PsyTest.ServiceIdentity.Application.ConfirmPasswordChange
{
    public record ConfirmPasswordChangeCommand(
        string Email,
        string Token,
        string NewPassword
    ) : IRequest<AppResult>;
}
