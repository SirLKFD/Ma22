using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Admin Controller
    /// </summary>
    public class AdminController : ControllerBase<AdminController>
    {
        private readonly IUserService _userService;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        public AdminController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration, IUserService userService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>

        [Authorize(Roles = "0")]
        public IActionResult UserMaster()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult CreateUser(AdminCreateUserViewModel model, IFormFile ProfilePicture, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ProfilePicture != null && ProfilePicture.Length > 0)
                    {
                        using var stream = ProfilePicture.OpenReadStream();

                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(ProfilePicture.FileName, stream),
                            Folder = "user_profiles"
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        // 🔍 Log or debug the result
                        if (uploadResult.Error != null)
                        {
                            Console.WriteLine("❌ Upload failed: " + uploadResult.Error.Message);
                            throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                        }
                        else
                        {
                            Console.WriteLine("✅ Upload succeeded");
                            Console.WriteLine("Secure URL: " + uploadResult.SecureUrl);

                            model.ProfilePicture = uploadResult.SecureUrl?.ToString(); // safe assignment
                        }

                    }

                    _userService.AddUser(model);

                    ViewBag.NewUser = model;
                    ViewBag.NewUserProfilePicture = model.ProfilePicture;
                    return View("UserMaster", model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View("UserMaster", model);
        }
    }
}
