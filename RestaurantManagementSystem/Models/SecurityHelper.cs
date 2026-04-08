using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace RestaurantManagementSystem.Models
{
    public static class SecurityHelper
    {
        private const int Iterations = 100000;
        private const int KeySize = 32; // 256 / 8

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(salt); }

            byte[] hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            // Lưu dưới dạng: Salt.Hash (ngăn cách bởi dấu chấm)
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split('.');
                if (parts.Length != 2) return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                byte[] generatedHash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

                return generatedHash.SequenceEqual(storedHash);
            }
            catch { return false; }
        }
    }
}
