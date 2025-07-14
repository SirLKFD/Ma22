using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<(bool Success, string Message)> RegisterPendingUserAsync(UserViewModel model, string token, string verificationLink);
        Task<(bool Success, string ErrorMessage)> VerifyEmailAsync(string token);
    }
} 