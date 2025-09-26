using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Features.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<String?>;
}
