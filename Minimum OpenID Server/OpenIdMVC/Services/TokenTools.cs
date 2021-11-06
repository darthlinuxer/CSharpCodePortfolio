using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace OpenIDAppMVC.Services
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
        public string CreateIdToken(ClaimsIdentity user, string issuer = "OpenID_Server", string audience = "TCPO", SecurityTokenDescriptor tokenDescriptor = null)
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
        public string CreateAccessToken(string username, string secretKey, string ip, string userAgent, long ticks)
        {
            string textToEncrypt = string.Join(":", new string[] { username, ip, userAgent });
            using HMAC hmac = HMACSHA256.Create(_alg);
            hmac.Key = Encoding.UTF8.GetBytes(GetHashedPassword(secretKey));
            hmac.ComputeHash(Encoding.UTF8.GetBytes(textToEncrypt));
            string hashedCode = Convert.ToBase64String(hmac.Hash);
            string hashedParams = string.Join(":", new string[] { username, ticks.ToString() });
            return string.Join(":", hashedCode, hashedParams).EncodeTo64();
        }

        public string GetHashedPassword(string password)
        {
            string _salt = Secret.Get("Salt");
            string passwordToEncrypt = string.Join(":", new string[] { password, _salt });

            using HMAC hmac = HMACSHA256.Create(_alg);
            // Hash the key.
            hmac.Key = Encoding.UTF8.GetBytes(_salt);
            hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordToEncrypt));
            return Convert.ToBase64String(hmac.Hash);
        }

        public static string[] DecodeTokenAndSeparateParts(string token)
        {
             string key = token.DecodeFrom64();
            // Split the parts.
            string[] parts = key.Split(new char[] { ':' });
            return parts;
        }

        private const int _expirationMinutes = 10;
        public bool IsTokenValid(string token, string ip, string userAgent)
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

                var secretKey = Secret.Get("PasswordHash");

                // Ensure the timestamp is valid.
                var elapsedTime = Math.Abs((DateTime.UtcNow - timeStamp).TotalMinutes);
                //bool expired = elapsedTime > _expirationMinutes;
                //if (expired) return false;
                var computedToken = CreateAccessToken(username, secretKey, ip, userAgent, ticks);
                result = (computedToken == token);
            }
            catch
            {
            }

            return result;
        }


    }
}