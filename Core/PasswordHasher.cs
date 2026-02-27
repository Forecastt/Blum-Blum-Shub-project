using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blum_Blum_Shub_project.Core
{
    public static class PasswordHasher
    {
        public static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string saltBase64)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] passBytes = Encoding.UTF8.GetBytes(password);

            byte[] data = new byte[salt.Length + passBytes.Length];
            Buffer.BlockCopy(salt, 0, data, 0, salt.Length);
            Buffer.BlockCopy(passBytes, 0, data, salt.Length, passBytes.Length);

            byte[] hash = SHA256.HashData(data);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string saltBase64, string expectedHashBase64)
        {
            string actual = HashPassword(password, saltBase64);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(actual),
                Convert.FromBase64String(expectedHashBase64)
            );
        }
    }
}
