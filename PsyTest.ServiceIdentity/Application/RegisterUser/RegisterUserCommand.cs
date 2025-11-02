using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Application.RegisterUser
{
    public record RegisterUserCommand(
        string UserName,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? PhoneNumber) : IRequest<IdentityResult>;
}
