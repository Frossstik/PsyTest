using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PsyTest.ServiceIdentity.Common;
using PsyTest.ServiceIdentity.Domain.Entities;

namespace PsyTest.ServiceIdentity.Application.SendChangePasswordEmail
{
    public class SendChangePasswordEmailHandler
        : IRequestHandler<SendChangePasswordEmailCommand, AppResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public SendChangePasswordEmailHandler(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<AppResult> Handle(SendChangePasswordEmailCommand request, CancellationToken ct)
        {
            // 1. кто меняет пароль
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                return AppResult.Fail("Пользователь не найден.");

            // 2. проверяем текущий пароль
            var valid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!valid)
                return AppResult.Fail("Неверный текущий пароль.");

            // 3. токен от Identity
            var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 4. кодируем всё ОДИН раз
            var encodedEmail = WebUtility.UrlEncode(user.Email!);
            var encodedToken = WebUtility.UrlEncode(rawToken);
            var encodedPassword = WebUtility.UrlEncode(request.NewPassword);

            // ⚠ ВАЖНО:
            // request.ConfirmUrl теперь = публичный URL фронта, например
            //    https://a54a2b5cf526.ngrok-free.app/confirm-password-change
            // (только без query. Мы соберём query здесь)
            var url =
                $"{request.ConfirmUrl}" +
                $"?email={encodedEmail}" +
                $"&token={encodedToken}" +
                $"&newPassword={encodedPassword}";

            var html = $@"
<p>Вы запросили смену пароля для <b>{WebUtility.HtmlEncode(user.Email)}</b>.</p>
<p>Чтобы подтвердить смену пароля, перейдите по ссылке:</p>
<p><a href=""{WebUtility.HtmlEncode(url)}"">Подтвердить смену пароля</a></p>
<p>Если это были не вы — проигнорируйте письмо.</p>";

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Подтверждение смены пароля",
                html
            );

            return AppResult.Ok("Письмо с подтверждением отправлено на почту.");
        }
    }
}
