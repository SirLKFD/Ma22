using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        /// <param name="passwordResetService">The password reset service.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="logger">The logger.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory,
                            IPasswordResetService passwordResetService,
                            IEmailService emailService,
                            ILogger<AccountController> logger) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._passwordResetService = passwordResetService;
            this._emailService = emailService;
            this._logger = logger;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(RoleBasedFilterService))]
        public ActionResult Login()
        {
            // Check if user is already authenticated
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                // Redirect based on role
                switch (role)
                {
                    case "0": // Admin role
                        return RedirectToAction("AdminTrainingCategory", "AdminTrainingCategory");

                    case "1": // User role
                        return RedirectToAction("UserDashboard", "User");
                    case "2": // User role
                        return RedirectToAction("UserMaster", "Admin");
                    default:
                        return RedirectToAction("Login", "Account");
                }
            }

            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }


        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Account account = null;

            // Authenticate user
            var loginResult = _userService.AuthenticateUser(model.EmailId, model.Password, ref account);

            if (loginResult == LoginResult.Success && account != null)
            {
                // Sign in
                await _signInManager.SignInAsync(account, true);

                // Set session values
                _session.SetString("UserName", $"{account.FirstName} {account.LastName}");
                _session.SetString("UserEmail", account.EmailId);
                _session.SetInt32("AccountId", account.Id);
                _session.SetInt32("AccountRole",account.Role);
                _session.SetString("ProfilePicture", account.ProfilePicture ?? "");

                // Redirect based on role
                switch ((RoleType)account.Role)
                {
                    case RoleType.Admin:
                        return RedirectToAction("AdminTrainingCategory", "AdminTrainingCategory");

                    case RoleType.User:
                        return RedirectToAction("UserDashboard", "User"); // Replace with actual User landing view
                    case RoleType.SuperAdmin:
                        return RedirectToAction("UserMaster", "Admin");

                    default:
                        TempData["ErrorMessage"] = "Incorrect account role.";
                        return View(model);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect Email or Password.";
                return View(model);
            }
        }



        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(RoleBasedFilterService))]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            try
            {
                _userService.AddUser(model);
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }

            return View(model);
        }


        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        //Forgot Password TEMPORARY MOCKUP
        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(RoleBasedFilterService))]
        public IActionResult ForgotPassword(string email = "")
        {
            var model = new PasswordResetRequestViewModel();
            if (!string.IsNullOrEmpty(email))
            {
                model.Email = email;
                TempData["SuccessMessage"] = "No worries, we sent you a reset link in your email.";
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(PasswordResetRequestViewModel model)
        {
            _logger.LogInformation("ForgotPassword POST called for email: {Email}", model.Email);
            if (ModelState.IsValid)
            {
                var token = await _passwordResetService.GenerateResetTokenAsync(model.Email);
                if (token != null)
                {
                    var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
                    var dynamicData = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "FirstName", model.Email }, // Replace with actual first name if available
                        { "ResetLink", resetLink }
                    };
                    _logger.LogInformation("Sending reset email to {Email} with link: {ResetLink}", model.Email, resetLink);
                    await _emailService.SendEmailAsync(model.Email, "d-7ee0cdbc2a14494faf8e4ba28c12af82", dynamicData);
                    TempData["SuccessMessage"] = "No worries, we sent you a reset link in your email.";
                }
                else
                {
                    _logger.LogWarning("ForgotPassword: Email not found: {Email}", model.Email);
                    TempData["ErrorMessage"] = "Email not found.";
                }
                return View(model);
            }
            else
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("ModelState error for key '{Key}': {Error}", key, error.ErrorMessage);
                    }
                }
                _logger.LogWarning("ForgotPassword: Invalid model state for email: {Email}", model.Email);
            }
            return View(model);
        }

        // Reset Password TEMPORARY MOCKUP
        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(RoleBasedFilterService))]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token) || !await _passwordResetService.ValidateResetTokenAsync(token))
            {
                TempData["ErrorMessage"] = "Invalid or expired reset token.";
                return RedirectToAction("Login");
            }
            var model = new ASI.Basecode.Services.ServiceModels.PasswordResetViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ASI.Basecode.Services.ServiceModels.PasswordResetViewModel model)
        {
            _logger.LogInformation("ResetPassword POST called for token: {Token}", model.Token);
            if (!ModelState.IsValid || model.NewPassword != model.ConfirmPassword)
            {
                _logger.LogWarning("ResetPassword: Passwords do not match or model invalid for token: {Token}", model.Token);
                TempData["ErrorMessage"] = "Passwords do not match.";
                return View(model);
            }
            var encryptedPassword = ASI.Basecode.Services.Manager.PasswordManager.EncryptPassword(model.NewPassword);
            var result = await _passwordResetService.ResetPasswordAsync(model.Token, encryptedPassword);
            if (result)
            {
                var resetTokenRepo = (ASI.Basecode.Data.Interfaces.IPasswordResetTokenRepository)HttpContext.RequestServices.GetService(typeof(ASI.Basecode.Data.Interfaces.IPasswordResetTokenRepository));
                var tokenEntity = resetTokenRepo.GetByToken(model.Token);
                var userEmail = tokenEntity?.Account?.EmailId;
                var firstName = tokenEntity?.Account?.FirstName ?? "User";
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var loginLink = Url.Action("Login", "Account", null, Request.Scheme);
                    var dynamicData = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "FirstName", firstName },
                        { "LoginLink", loginLink }
                    };
                    _logger.LogInformation("Sending password reset confirmation email to {Email}", userEmail);
                    await _emailService.SendEmailAsync(userEmail, "d-34c6128ba49d493895e13800334887ce", dynamicData);
                }
                TempData["SuccessMessage"] = "Password updated successfully.";
                _logger.LogInformation("Password reset successful for token: {Token}", model.Token);
                return View(model);
            }
            _logger.LogWarning("ResetPassword: Invalid or expired reset token: {Token}", model.Token);
            TempData["ErrorMessage"] = "Invalid or expired reset token.";
            return View(model);
        }

    }
}
