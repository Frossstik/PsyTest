using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Features.RegisterUser
{
    public record RegisterUserCommand(string Email, string Password) : IRequest<IdentityResult>;
}
