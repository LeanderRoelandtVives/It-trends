using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantAi.Dto.Request;
using RestaurantAi.Model;
using RestaurantAi.Model.Security;
using RestaurantAI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantAI.Services.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtsettings) : IAuthService
    {

        public async Task<string> RegisterAsync(RegisterRequest registerRequest)
        {
            var user = new ApplicationUser
            {
                UserName = registerRequest.Email,
                Email = registerRequest.Email,
                FullName = registerRequest.FullName
            };

            var result = await userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));

            return await GenerateJwt(user);
        }

        public async Task<string> LoginAsync(LoginRequest loginRequest)
        {
            var user = await userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginRequest.Password))
                throw new Exception("Invalid credentials");

            return await GenerateJwt(user);
        }

        private async Task<string> GenerateJwt(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtsettings.Value.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtsettings.Value.Issuer,
                audience: jwtsettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtsettings.Value.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GoogleLoginAsync(string email, string fullName)
        {
            // Find existing user or create new one
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName ?? email,
                    EmailConfirmed = true // Google already verified the email
                };
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                    throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));
            }
            return await GenerateJwt(user); // reuses your existing method
        }
    }
}