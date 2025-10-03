using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Features.UpdateProfile
{
    public record UpdateProfileCommand(
        string UserId, 
        string FirstName, 
        string LastName, 
        string? PhoneNumber, 
        string? Email, 
        string? Password) : IRequest<IdentityResult>;
}
