using MediatR;
using Microsoft.AspNetCore.Identity;

namespace PsyTest.ServiceIdentity.Application.UpdateProfile
{
    public record UpdateProfileCommand(
        string UserId, 
        string FirstName, 
        string LastName, 
        string? PhoneNumber, 
        string? Email) : IRequest<IdentityResult>;
}
