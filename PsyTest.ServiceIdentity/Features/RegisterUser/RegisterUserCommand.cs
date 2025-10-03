using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Features.RegisterUser
{
    public record RegisterUserCommand(
        string UserName,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? PhoneNumber) : IRequest<IdentityResult>;
}
