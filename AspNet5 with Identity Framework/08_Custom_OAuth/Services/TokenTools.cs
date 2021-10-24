using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Services
{
    public class TokenTools
    {
        public TokenValidationParameters tokenParameters;
        private readonly IConfiguration configuration;
        public TokenTools(
            IConfiguration configuration,
            string issuer = "Reason Systems",
            TokenValidationParameters tokenParameters = null)
        {
            this.configuration = configuration;
            this.tokenParameters = tokenParameters ?? new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidateIssuer = true,
                IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("SecretKey"))),
                ClockSkew = TimeSpan.Zero // remove delay of token when expire
            };
        }
        public string CreateToken(ClaimsIdentity user, string issuer = "Reason Systems", string audience = "TCPO", SecurityTokenDescriptor tokenDescriptor = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var privateTokenKey =
            Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey"));
            var _tokenDescriptor = tokenDescriptor ?? new SecurityTokenDescriptor()
            {
                Subject = user,
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(privateTokenKey),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow
            };
            var token = tokenHandler.CreateToken(_tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ExtractPrincipal(
            string token,
            string issuer = "Reason Systems")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenParameters.ValidIssuer = issuer;
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(
                token, tokenParameters, out _);
            return claimsPrincipal;
        }


    }
}