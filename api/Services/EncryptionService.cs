using System.Security.Cryptography;
using System.Text;

namespace UPTRMS.Api.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        var encryptionKey = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key not configured");

        // Derive key and IV from configuration
        using var sha256 = SHA256.Create();
        var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
        _key = keyBytes.Take(32).ToArray(); // 256-bit key
        _iv = keyBytes.Skip(16).Take(16).ToArray(); // 128-bit IV
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = Encrypt(plainBytes);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = Decrypt(cipherBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] Encrypt(byte[] plainData)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
    }

    public byte[] Decrypt(byte[] cipherData)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
    }
}