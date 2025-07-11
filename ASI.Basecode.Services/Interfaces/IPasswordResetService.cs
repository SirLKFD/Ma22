using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetTokenAsync(string email);
        Task<string> GenerateVerificationTokenAsync(string email);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> VerifyEmailAsync(string token);
    }
} 