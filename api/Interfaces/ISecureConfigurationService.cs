namespace TherapyDocs.Api.Interfaces;

public interface ISecureConfigurationService
{
    string GetConnectionString(string name);
    string EncryptConnectionString(string connectionString);
}