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
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Генерируем токен подтверждения email
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Здесь должна быть логика отправки email
                // Например, через EmailService, RabbitMQ и т.д.
                Console.WriteLine($"Email confirmation token for {user.Email}: {emailToken}");

                // Если указан телефон, генерируем токен для подтверждения телефона
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var phoneToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);
                    // Логика отправки SMS
                    Console.WriteLine($"Phone confirmation token for {user.PhoneNumber}: {phoneToken}");
                }
            }

            return result;
        }
    }
}