using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Data.ValueConverters;

public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IEncryptionService encryptionService)
        : base(
            v => Encrypt(v, encryptionService),
            v => Decrypt(v, encryptionService))
    {
    }

    private static string Encrypt(string value, IEncryptionService encryptionService)
    {
        return string.IsNullOrEmpty(value) ? value : encryptionService.Encrypt(value);
    }

    private static string Decrypt(string value, IEncryptionService encryptionService)
    {
        return string.IsNullOrEmpty(value) ? value : encryptionService.Decrypt(value);
    }
}