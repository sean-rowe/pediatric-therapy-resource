namespace TherapyDocs.Api.Interfaces;

public interface IHaveIBeenPwnedService
{
    Task<bool> IsPasswordPwnedAsync(string password);
}