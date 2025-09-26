using MediatR;
using Microsoft.AspNetCore.Identity;
using PsyTest.ServiceIdentity.Entities;

namespace PsyTest.ServiceIdentity.Features.RegisterUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            return await _userManager.CreateAsync(user, request.Password);
        }
    }
}
