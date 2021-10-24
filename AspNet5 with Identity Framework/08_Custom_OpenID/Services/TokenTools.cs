using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Services
{
    public class TokenTools
    {
        public TokenValidationParameters tokenParameters;
        private SecretService Secret {get; set;} 
        public TokenTools(
            SecretService secret,
            string issuer = "Reason Systems",
            TokenValidationParameters tokenParameters = null)
        {
            this.Secret = secret;
            
            this.tokenParameters = tokenParameters ?? new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidateIssuer = true,
                IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret.Get("SecretKey"))),
                ClockSkew = TimeSpan.Zero // remove delay of token when expire
            };
        }
        public string CreateIdToken(ClaimsIdentity user, string issuer = "Reason Systems", string audience = "TCPO", SecurityTokenDescriptor tokenDescriptor = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var privateTokenKey =
            Encoding.ASCII.GetBytes(Secret.Get("SecretKey"));
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

        private const string _alg = "HmacSHA256";
        public string CreateAccessToken(string username, string password, string ip, string userAgent, long ticks)
        {
            string hash = string.Join(":", new string[] { username, ip, userAgent, ticks.ToString() });
            using HMAC hmac = HMACSHA256.Create(_alg);
            hmac.Key = Encoding.UTF8.GetBytes(GetHashedPassword(password));
            hmac.ComputeHash(Encoding.UTF8.GetBytes(hash));
            string hashLeft = Convert.ToBase64String(hmac.Hash);
            string hashRight = string.Join(":", new string[] { username, ticks.ToString() });
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(string.Join(":", hashLeft, hashRight)));
        }

        public string GetHashedPassword(string password)
        {
            string _salt = Secret.Get("Salt");
            string key = string.Join(":", new string[] { password, _salt });

            using HMAC hmac = HMACSHA256.Create(_alg);
            // Hash the key.
            hmac.Key = Encoding.UTF8.GetBytes(_salt);
            hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
            return Convert.ToBase64String(hmac.Hash);
        }

        public static string[] DecodeTokenAndSeparateParts(string token)
        {
             string key = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            // Split the parts.
            string[] parts = key.Split(new char[] { ':' });
            return parts;
        }

        private const int _expirationMinutes = 10;
        public bool IsTokenValid(string token, string ip, string userAgent, string password)
        {
            bool result = false;
            try
            {
                string[] parts = DecodeTokenAndSeparateParts(token);
                if (parts.Length != 3) return false;

                // Get the hash message, username, and timestamp.
                string hash = parts[0];
                string username = parts[1];
                long ticks = long.Parse(parts[2]);
                DateTime timeStamp = new(ticks);

                // Ensure the timestamp is valid.
                var elapsedTime = Math.Abs((DateTime.UtcNow - timeStamp).TotalMinutes);
                bool expired = elapsedTime < _expirationMinutes;
                if (expired) return false;
                var computedToken = CreateAccessToken(username, password, ip, userAgent, ticks);
                result = (computedToken == token);
            }
            catch
            {
            }

            return result;
        }


    }
}