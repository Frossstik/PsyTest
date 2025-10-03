using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using PsyTest.ServiceIdentity.Entities;
using System.Security.Claims;
using System.Text;

namespace PsyTest.ServiceIdentity.Common
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
            Console.WriteLine("JWT Key: " + _config["Jwt:Key"]);
            Console.WriteLine("Token exp: " + DateTimeOffset.FromUnixTimeSeconds(1759334980).UtcDateTime);
            Console.WriteLine("Now UTC: " + DateTimeOffset.UtcNow);
        }

        public string Generate(ApplicationUser user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // ID
            new Claim(JwtRegisteredClaimNames.Email, user.Email),        // email
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),   // чтобы User.Identity.Name работал
            new Claim("role", "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);



            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}