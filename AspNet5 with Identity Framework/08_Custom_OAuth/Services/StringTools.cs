using System;
using System.Security.Cryptography;

namespace OAuthApp.Services
{
    public class RandomPassword
    {
        public static string Generate(int length)
        {
            byte[] rgb = new byte[length];
            RNGCryptoServiceProvider rngCrypt = new();
            rngCrypt.GetBytes(rgb);
            return Convert.ToBase64String(rgb);
        }
    }
}