using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PsyTest.ServiceIdentity.Common;
using PsyTest.ServiceIdentity.Domain.Entities;

namespace PsyTest.ServiceIdentity.Application.ConfirmPasswordChange
{
    public class ConfirmPasswordChangeHandler
        : IRequestHandler<ConfirmPasswordChangeCommand, AppResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmPasswordChangeHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AppResult> Handle(ConfirmPasswordChangeCommand request, CancellationToken ct)
        {
            // 1. находим юзера
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return AppResult.Fail("Пользователь не найден.");

            // 2. раскодировать один раз
            var tokenDecoded = WebUtility.UrlDecode(request.Token);
            var newPassDecoded = WebUtility.UrlDecode(request.NewPassword);

            // 3. применить ResetPasswordAsync
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return AppResult.Fail(errors);
            }

            return AppResult.Ok("Пароль успешно изменён.");
        }
    }
}
