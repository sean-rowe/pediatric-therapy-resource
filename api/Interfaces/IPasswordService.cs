namespace TherapyDocs.Api.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool IsCommonPassword(string password);
}