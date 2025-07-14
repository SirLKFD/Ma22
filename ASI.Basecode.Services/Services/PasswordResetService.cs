using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public PasswordResetService(IUnitOfWork unitOfWork, IPasswordResetTokenRepository tokenRepo, IUserRepository userRepo, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _tokenRepo = tokenRepo;
            _userRepo = userRepo;
            _config = config;
            _logger = loggerFactory.CreateLogger<PasswordResetService>();
        }

        public async Task<string> GenerateResetTokenAsync(string email)
        {
            _logger.LogInformation("Generating reset token for email: {Email}", email);
            var user = _userRepo.GetUsers().FirstOrDefault(u => u.EmailId == email);
            if (user == null) {
                _logger.LogWarning("No user found for email: {Email}", email);
                return null;
            }
            var token = GenerateSecureToken();
            var expiration = DateTime.UtcNow.AddHours(24);
            var resetToken = new PasswordResetToken
            {
                AccountId = user.Id,
                Token = token,
                ExpirationTime = expiration,
                IsUsed = false
            };
            _tokenRepo.AddToken(resetToken);
            _unitOfWork.SaveChanges();
            _logger.LogInformation("Reset token generated and saved for userId: {UserId}", user.Id);
            return token;
        }

        public async Task<string> GenerateVerificationTokenAsync(string email)
        {
            _logger.LogInformation("Generating verification token for email: {Email}", email);
            var token = GenerateSecureToken();
            var expiration = DateTime.UtcNow.AddHours(1);
    
            var resetToken = new PasswordResetToken
            {
                // Special value for email verification
                Token = token,
                ExpirationTime = expiration,
                IsUsed = false
            };
            _tokenRepo.AddToken(resetToken);
            _unitOfWork.SaveChanges();
            _logger.LogInformation("Verification token generated for email: {Email}", email);
            return token;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            _logger.LogInformation("Validating reset token: {Token}", token);
            var resetToken = _tokenRepo.GetByToken(token);
            if (resetToken == null || resetToken.IsUsed == true || resetToken.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired reset token: {Token}", token);
                return false;
            }
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            _logger.LogInformation("Resetting password for token: {Token}", token);
            var resetToken = _tokenRepo.GetByToken(token);
            if (resetToken == null || resetToken.IsUsed == true || resetToken.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired reset token during password reset: {Token}", token);
                return false;
            }
            var user = _userRepo.GetUsers().FirstOrDefault(u => u.Id == resetToken.AccountId);
            if (user == null) {
                _logger.LogWarning("No user found for reset token: {Token}", token);
                return false;
            }
            user.Password = newPassword;
            resetToken.IsUsed = true;
            _tokenRepo.UpdateToken(resetToken);
            _userRepo.UpdateUser(user);
            _unitOfWork.SaveChanges();
            _logger.LogInformation("Password reset successful for userId: {UserId}", user.Id);
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            _logger.LogInformation("Verifying email for token: {Token}", token);
            var resetToken = _tokenRepo.GetByToken(token);
            if (resetToken == null || resetToken.IsUsed == true || resetToken.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired verification token: {Token}", token);
                return false;
            }
            
            // Mark token as used (email verified)
            resetToken.IsUsed = true;
            _tokenRepo.UpdateToken(resetToken);
            _unitOfWork.SaveChanges();
            return true;
        }

        private string GenerateSecureToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
            }
        }
    }
} 