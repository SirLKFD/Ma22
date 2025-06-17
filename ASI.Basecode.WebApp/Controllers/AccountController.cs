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
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
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
                        return RedirectToAction("UserMaster", "Admin");

                    case "1": // User role
                        return RedirectToAction("UserDashboard", "User");

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
                await _signInManager.SignInAsync(account);

                // Set session values
                _session.SetString("UserName", $"{account.FirstName} {account.LastName}");
                _session.SetString("UserEmail", account.EmailId);

                // Redirect based on role
                switch ((RoleType)account.Role)
                {
                    case RoleType.Admin:
                        return RedirectToAction("UserMaster", "Admin");

                    case RoleType.User:
                        return RedirectToAction("UserDashboard", "User"); // Replace with actual User landing view

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
            try
            {
                _userService.AddUser(model);
                return RedirectToAction("Login", "Account");
            }
            catch(InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View();
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
            var model = new LoginViewModel();

            // If email is provided from login page, pre-fill it
            if (!string.IsNullOrEmpty(email))
            {
                model.EmailId = email;
                // Show success message if email was provided
                TempData["SuccessMessage"] = "No worries, we sent you a reset link in your email.";
            }

            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult ForgotPassword(LoginViewModel model)
        {
            // Use the existing Required validation from LoginViewModel
            if (ModelState.IsValid)
            {
                // For mockup - just show success message
                TempData["SuccessMessage"] = "No worries, we sent you a reset link in your email.";
                return View(model);
            }

            // ModelState will automatically handle the Required validation error from LoginViewModel
            return View(model);
        }

        // Reset Password TEMPORARY MOCKUP
        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(RoleBasedFilterService))]
        public IActionResult ResetPassword()
        {
            return View();
        }


    }
}
