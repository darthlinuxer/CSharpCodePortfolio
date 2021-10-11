using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using crypto;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.TokenTools
{
    public class TokenTools
    {   
        public TokenValidationParameters tokenParameters;
        private readonly IConfiguration configuration;
        public TokenTools(IConfiguration configuration) {
            this.configuration = configuration;
            tokenParameters = new TokenValidationParameters
            {
                ValidIssuer = "Reason Systems",
                ValidAudience = "TCPO",
                ValidateAudience = true,
                ValidateIssuer = true,
                IssuerSigningKey = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("SecretKey"))),
                ClockSkew = TimeSpan.Zero // remove delay of token when expire
            };
        }
        public string CreateToken(ClaimsIdentity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = 
            Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey"));
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = user,
                Expires = DateTime.UtcNow.AddHours(8),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), 
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer="Reason Systems",
                Audience="TCPO",
                IssuedAt= DateTime.UtcNow
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsIdentity ExtractIdentity(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            token = token.Remove(0,7);
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(
                token, tokenParameters, out _);
            return claimsPrincipal.Identity as ClaimsIdentity;
        }
    }
}