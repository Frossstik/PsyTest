using MediatR;
using PsyTest.ServiceIdentity.Common;

namespace PsyTest.ServiceIdentity.Application.SendChangePasswordEmail
{
    public record SendChangePasswordEmailCommand(
        string UserId,
        string CurrentPassword,
        string NewPassword,
        string ConfirmUrl
    ) : IRequest<AppResult>;
}
