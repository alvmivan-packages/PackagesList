using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PackagesList.TokenSecure
{
    public static class SecureTokenStorage
    {
        public static string EncryptToken(string token, string password)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using var aes = Aes.Create();
            using (var pdb = new Rfc2898DeriveBytes(password, passwordBytes))
            {
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);
            }

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(tokenBytes, 0, tokenBytes.Length);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string DecryptToken(string encryptedToken, string password)
        {
            byte[] encryptedTokenBytes = Convert.FromBase64String(encryptedToken);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (var aes = Aes.Create())
            {
                using (var pdb = new Rfc2898DeriveBytes(password, passwordBytes))
                {
                    aes.Key = pdb.GetBytes(32);
                    aes.IV = pdb.GetBytes(16);
                }

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedTokenBytes, 0, encryptedTokenBytes.Length);
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}