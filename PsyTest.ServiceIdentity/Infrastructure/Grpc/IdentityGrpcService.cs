using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PsyTest.identity;
using PsyTest.ServiceIdentity.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PsyTest.ServiceIdentity.Infrastructure.Grpc
{
    public class IdentityGrpcService : Identity.IdentityBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public IdentityGrpcService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
            Console.WriteLine("Token exp: " + DateTimeOffset.FromUnixTimeSeconds(1759334980).UtcDateTime);
            Console.WriteLine("Now UTC: " + DateTimeOffset.UtcNow);
        }

        public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                return new ValidateTokenResponse
                {
                    IsValid = true,
                    UserId = userId
                };
            }
            catch
            {
                return new ValidateTokenResponse { IsValid = false };
            }
        }
    }
}
