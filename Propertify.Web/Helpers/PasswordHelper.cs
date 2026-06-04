using System.Security.Cryptography;

namespace Propertify.Web.Helpers
{
    /// <summary>
    /// Provides PBKDF2-SHA256 password hashing and constant-time verification.
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        /// <summary>
        /// Hashes a plaintext password using PBKDF2-SHA256 with a random 16-byte salt.
        /// Returns a Base64 string containing the salt + hash (48 bytes total).
        /// </summary>
        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            byte[] combined = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);

            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Verifies a plaintext password against a stored hash produced by <see cref="Hash"/>.
        /// Uses constant-time comparison to prevent timing attacks.
        /// </summary>
        public static bool Verify(string password, string hashedPassword)
        {
            byte[] combined = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            byte[] expectedHash = new byte[HashSize];

            Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(combined, SaltSize, expectedHash, 0, HashSize);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
