using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
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
        public IActionResult CreateUser(AdminCreateUserViewModel model, IFormFile ProfilePicture)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Convert uploaded file to byte array
                    if (ProfilePicture != null && ProfilePicture.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            ProfilePicture.CopyTo(memoryStream);
                            model.ProfilePicture = memoryStream.ToArray();
                        }
                    }
                    
                    _userService.AddUser(model);

                /*     // Pangdisplay rani sa new user para makita nako if nigana ba iuncomment lang if u want
                    ViewBag.NewUser = model;
                    if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                    {
                        ViewBag.NewUserProfilePicture = $"data:image/png;base64,{Convert.ToBase64String(model.ProfilePicture)}";
                    }
                    else
                    {
                        ViewBag.NewUserProfilePicture = null;
                    } */
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
