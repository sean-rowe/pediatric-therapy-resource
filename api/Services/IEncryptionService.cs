namespace UPTRMS.Api.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    byte[] Encrypt(byte[] plainData);
    byte[] Decrypt(byte[] cipherData);
}