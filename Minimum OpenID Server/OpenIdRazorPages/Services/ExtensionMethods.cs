using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpenIDAppRazor.Services
{
    public static class StringExtensions
    {
        static public string EncodeTo64(this string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        static public string DecodeFrom64(this string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        static public string EncryptAES256(this string source, string password, string salt)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(source);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            // The salt bytes must be at least 8 bytes.
        
            // Hash the password with SHA256
            passwordBytes = SHA256.Create()
                                  .ComputeHash(passwordBytes);

            using MemoryStream ms = new();
            using RijndaelManaged AES = new();

            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CBC;

            using var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            cs.Close();

            byte[] encryptedBytes = ms.ToArray();
            return Convert.ToBase64String(encryptedBytes);
        }

        static public string DecryptAES256(this string encryptedResult, string password, string salt)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(encryptedResult);
            byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            passwordBytesdecrypt = SHA256.Create()
                                         .ComputeHash(passwordBytesdecrypt);
            
            using MemoryStream ms = new();
            using RijndaelManaged AES = new();

            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytesdecrypt, saltBytes, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CBC;

            using CryptoStream cs = new(ms, AES.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
            cs.Close();
            
            byte[] decryptedBytes = ms.ToArray();
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}