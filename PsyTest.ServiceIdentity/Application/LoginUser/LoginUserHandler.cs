using MediatR;
using Microsoft.AspNetCore.Identity;
using PsyTest.ServiceIdentity.Common;
using PsyTest.ServiceIdentity.Domain.Entities;

namespace PsyTest.ServiceIdentity.Application.LoginUser
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, string?>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenGenerator _tokenGenerator;

        public LoginUserHandler(UserManager<ApplicationUser> userManager, JwtTokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<string?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return null;

            return _tokenGenerator.Generate(user);
        }
    }
}
