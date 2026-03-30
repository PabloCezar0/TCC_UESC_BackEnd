using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Rota.Application.Services
{
    public class Argon2Hasher
    {
        private readonly int _degreeOfParallelism;
        private readonly int _memorySize;
        private readonly int _iterations;

        public Argon2Hasher(
            int degreeOfParallelism = 8,
            int memorySize = 65536,
            int iterations = 4)
        {
            _degreeOfParallelism = degreeOfParallelism;
            _memorySize = memorySize;
            _iterations = iterations;
        }

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _degreeOfParallelism,
                MemorySize = _memorySize,
                Iterations = _iterations
            };

            byte[] hash = argon2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
        }

        public bool LoginTest(string storedHash, string providedPassword)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            var hashParts = storedHash.Split('|', 2);
            if (hashParts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(hashParts[0]);
            byte[] originalHash = Convert.FromBase64String(hashParts[1]);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(providedPassword))
            {
                Salt = salt,
                DegreeOfParallelism = _degreeOfParallelism,
                MemorySize = _memorySize,
                Iterations = _iterations
            };

            byte[] newHash = argon2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
        }
    }
}