using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
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
        public PasswordResetService(IUnitOfWork unitOfWork, IPasswordResetTokenRepository tokenRepo, IUserRepository userRepo, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _tokenRepo = tokenRepo;
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<string> GenerateResetTokenAsync(string email)
        {
            var user = _userRepo.GetUsers().FirstOrDefault(u => u.EmailId == email);
            if (user == null) return null;

            var token = GenerateSecureToken();
            var expiration = DateTime.UtcNow.AddHours(24);
            var resetToken = new PasswordResetToken
            {
                AccountId = user.Id,
                Token = token,
                Expiration = expiration,
                IsUsed = false
            };
            _tokenRepo.AddToken(resetToken);
            _unitOfWork.SaveChanges();
            return token;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            var resetToken = _tokenRepo.GetByToken(token);
            if (resetToken == null || resetToken.IsUsed || resetToken.Expiration < DateTime.UtcNow)
                return false;
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = _tokenRepo.GetByToken(token);
            if (resetToken == null || resetToken.IsUsed || resetToken.Expiration < DateTime.UtcNow)
                return false;
            var user = _userRepo.GetUsers().FirstOrDefault(u => u.Id == resetToken.AccountId);
            if (user == null) return false;
            user.Password = newPassword; // Should be encrypted by PasswordManager before calling this
            resetToken.IsUsed = true;
            _tokenRepo.UpdateToken(resetToken);
            _userRepo.UpdateUser(user);
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