using System.Security.Cryptography;
using System.Text;
using ExpenseTracker.Application.Interfaces;
using Konscious.Security.Cryptography;

namespace ExpenseTracker.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 32; // 256 bit
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 2;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Hasło nie może być puste", nameof(password));

        // Generuj losową sól
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        
        // Hashuj hasło
        byte[] hash = HashPasswordWithSalt(password, salt);
        
        // Zwróć sól + hash jako base64
        byte[] hashWithSalt = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, hashWithSalt, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, hashWithSalt, SaltSize, HashSize);
        
        return Convert.ToBase64String(hashWithSalt);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Hasło nie może być puste", nameof(password));
        
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash nie może być pusty", nameof(hash));

        try
        {
            // Dekoduj hash
            byte[] hashWithSalt = Convert.FromBase64String(hash);
            
            if (hashWithSalt.Length != SaltSize + HashSize)
                return false;
            
            // Wyciągnij sól
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashWithSalt, 0, salt, 0, SaltSize);
            
            // Wyciągnij oryginalny hash
            byte[] originalHash = new byte[HashSize];
            Buffer.BlockCopy(hashWithSalt, SaltSize, originalHash, 0, HashSize);
            
            // Zhashuj podane hasło z tą samą solą
            byte[] testHash = HashPasswordWithSalt(password, salt);
            
            // Porównaj hashe w sposób odporny na timing attacks
            return CryptographicOperations.FixedTimeEquals(originalHash, testHash);
        }
        catch
        {
            return false;
        }
    }

    private byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };
        
        return argon2.GetBytes(HashSize);
    }
}
