using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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

        public IActionResult UserMaster(int page = 1, string filter = "all")
        {
            const int PageSize = 8;

            var allUsers = _userService.GetAllUsers().ToList();

            // Get filtered users based on query
            var filteredUsers = filter switch
            {
                "admin" => allUsers.Where(u => u.Role == 0).ToList(),
                "guest" => allUsers.Where(u => u.Role == 1).ToList(),
                _ => allUsers
            };

            // Paginate
            var paginated = PaginationIndexService.Paginate(filteredUsers, page, PageSize);

            var viewModel = new AdminCreateUserViewModel
            {
                Users = paginated.Items.Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    ProfilePicture = u.ProfilePicture, 
                    EmailId = u.EmailId,
                    Contact = u.Contact,
                    Birthdate = u.Birthdate,
                    CreatedTime = u.CreatedTime
                }).ToList(),

                CurrentPage = paginated.CurrentPage,
                TotalPages = paginated.TotalPages,

                // ✅ Count all, not just paginated
                TotalUsers = allUsers.Count,
                TotalAdmins = allUsers.Count(u => u.Role == 0),
                TotalGuests = allUsers.Count(u => u.Role == 1)
            };

            return View(viewModel);
        }


        public IActionResult ViewUserDetails(int userId)
        {
            var user = _userService.GetAllUsers().FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userModel = new UserListViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FirstName + " " + user.LastName,
                EmailId = user.EmailId,
                Role = user.Role,
                Contact = user.Contact,
                Birthdate = user.Birthdate,
                CreatedTime = user.CreatedTime,
                ProfilePicture = user.ProfilePicture
            };

            return PartialView("_UserViewModal", userModel); // return partial view instead of full view
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

                        if (uploadResult.Error != null)
                        {
                            throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                        }

                        model.ProfilePicture = uploadResult.SecureUrl?.ToString();
                    }

                    _userService.AddUser(model); // Insert to DB

                    TempData["Success"] = "User added successfully!";
                    return RedirectToAction("UserMaster"); // ✅ Redirect after success
                }
                catch (Exception ex)
                {
                    TempData["FormError"] = "Error: " + ex.Message;
                    return RedirectToAction("UserMaster");
                }
            }

            // If model is invalid, redirect with error
            TempData["FormError"] = "Validation failed. Please check your inputs.";
            return RedirectToAction("UserMaster");
        }

    }
}
