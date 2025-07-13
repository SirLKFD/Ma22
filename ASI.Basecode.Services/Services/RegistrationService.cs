using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IPendingUserRegistrationRepository _pendingRepo;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;

        public RegistrationService(IPendingUserRegistrationRepository pendingRepo, IUserService userService, IEmailService emailService, IAuditLogService auditLogService)
        {
            _pendingRepo = pendingRepo;
            _userService = userService;
            _emailService = emailService;
            _auditLogService = auditLogService;
        }

        public async Task<(bool Success, string Message)> RegisterPendingUserAsync(UserViewModel model, string token, string verificationLink)
        {
            var existingUser = _userService.GetUserByEmail(model.EmailId);
            var pending = _pendingRepo.GetByEmail(model.EmailId);
            bool hasActivePending = pending != null && !pending.IsVerified && pending.TokenExpiration > DateTime.UtcNow;

            if (existingUser != null)
                return (false, "An account with this email already exists.");

            if (hasActivePending)
                return (false, "A registration for this email is already pending. Please check your email for the verification link or wait for the token to expire.");

            var pendingUser = new PendingUserRegistration
            {
                EmailId = model.EmailId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Contact = model.Contact,
                PasswordHash = PasswordManager.EncryptPassword(model.Password),
                VerificationToken = token,
                TokenExpiration = DateTime.UtcNow.AddHours(24),
                IsVerified = false
            };
            _pendingRepo.Add(pendingUser);
            _pendingRepo.Save();

            var dynamicData = new System.Collections.Generic.Dictionary<string, string>
            {
                { "FirstName", model.FirstName },
                { "AddUser", verificationLink }
            };
            await _emailService.SendEmailAsync(model.EmailId, "d-f2ac2a117037485cb26979d784ab74f3", dynamicData);

            return (true, "Registration successful! Please check your email to verify your account.");
        }

        public async Task<(bool Success, string ErrorMessage)> VerifyEmailAsync(string token)
        {
            var pendingUser = _pendingRepo.GetByToken(token);
            if (pendingUser == null || pendingUser.IsVerified)
                return (false, "Invalid or expired verification token.");

            if (pendingUser.TokenExpiration < DateTime.UtcNow)
                return (false, "Verification token has expired.");

            // Create user account
            var userViewModel = new UserViewModel
            {
                EmailId = pendingUser.EmailId,
                FirstName = pendingUser.FirstName,
                LastName = pendingUser.LastName,
                Contact = pendingUser.Contact,
                Password = pendingUser.PasswordHash,
                ConfirmPassword = pendingUser.PasswordHash
            };
            _userService.AddUser(userViewModel, true);
            // Log the AddUser action
            string fullName = $"{pendingUser.FirstName} {pendingUser.LastName}";
            // Get the newly created user to get their ID
            var newUser = _userService.GetUserByEmail(pendingUser.EmailId);
            _auditLogService.LogAction("User", "Create", fullName, newUser.Id, fullName);

            pendingUser.IsVerified = true;
            _pendingRepo.Update(pendingUser);
            _pendingRepo.Save();

            return (true, null);
        }
    }
} 