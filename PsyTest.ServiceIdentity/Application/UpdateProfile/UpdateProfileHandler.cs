using MediatR;
using Microsoft.AspNetCore.Identity;
using PsyTest.ServiceIdentity.Domain.Entities;

namespace PsyTest.ServiceIdentity.Application.UpdateProfile
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateProfileHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
                user.Email = request.Email;

            var result = await _userManager.UpdateAsync(user);

            return result;
        }
    }
}
