using UPTRMS.Api.Services;

namespace UPTRMS.Api.Tests.BDD.Mocks;

public class MockEncryptionService : IEncryptionService
{
    private const string Prefix = "ENCRYPTED:";
    
    public string Encrypt(string plainText)
    {
        // For testing, just return a base64 encoded version
        if (string.IsNullOrEmpty(plainText))
            return plainText;
        
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{Prefix}{plainText}"));
    }

    public string Decrypt(string cipherText)
    {
        // For testing, just decode the base64
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;
        
        var bytes = Convert.FromBase64String(cipherText);
        var decrypted = System.Text.Encoding.UTF8.GetString(bytes);
        
        if (decrypted.StartsWith(Prefix))
            return decrypted.Substring(Prefix.Length);
        
        return decrypted;
    }
    
    public byte[] Encrypt(byte[] plainData)
    {
        // For testing, just add a simple prefix
        if (plainData == null || plainData.Length == 0)
            return plainData;
        
        var prefixBytes = System.Text.Encoding.UTF8.GetBytes(Prefix);
        var result = new byte[prefixBytes.Length + plainData.Length];
        Array.Copy(prefixBytes, 0, result, 0, prefixBytes.Length);
        Array.Copy(plainData, 0, result, prefixBytes.Length, plainData.Length);
        return result;
    }
    
    public byte[] Decrypt(byte[] cipherData)
    {
        // For testing, just remove the prefix
        if (cipherData == null || cipherData.Length == 0)
            return cipherData;
        
        var prefixBytes = System.Text.Encoding.UTF8.GetBytes(Prefix);
        if (cipherData.Length >= prefixBytes.Length)
        {
            // Check if data starts with prefix
            bool hasPrefix = true;
            for (int i = 0; i < prefixBytes.Length; i++)
            {
                if (cipherData[i] != prefixBytes[i])
                {
                    hasPrefix = false;
                    break;
                }
            }
            
            if (hasPrefix)
            {
                var result = new byte[cipherData.Length - prefixBytes.Length];
                Array.Copy(cipherData, prefixBytes.Length, result, 0, result.Length);
                return result;
            }
        }
        
        return cipherData;
    }
}