using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        public string GenerateJSONWebToken(Customer userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyNopAbandonedCartsKey"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username ?? "Guest"),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email ?? "Guest"),
                new Claim("CreatedOnUtc", userInfo.CreatedOnUtc.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken("NopStation.com",
                "NopStation.com",
                claims,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
