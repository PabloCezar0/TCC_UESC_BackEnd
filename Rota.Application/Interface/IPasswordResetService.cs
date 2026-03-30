namespace Rota.Application.Interfaces;

public interface IPasswordResetService
{
    Task RequestResetAsync(string email);
    Task<bool> ConfirmResetAsync(string token, string newPassword);
}
