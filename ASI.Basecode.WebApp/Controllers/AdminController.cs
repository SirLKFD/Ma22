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
        private readonly IAuditLogService _auditLogService;
        

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
                              IAuditLogService auditLogService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>

     
        [Authorize(Roles="2")]
        public IActionResult UserMaster(int page = 1, string filter = "all", string search = "")
        {
            const int PageSize = 7;

            var allUsers = _userService.GetAllUsers().ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                allUsers = allUsers.Where(u =>
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.EmailId) && u.EmailId.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    ($"{u.FirstName} {u.LastName}").Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var filteredUsers = filter switch
            {
                "admin" => allUsers.Where(u => u.Role == 0).ToList(),
                "guest" => allUsers.Where(u => u.Role == 1).ToList(),
                _ => allUsers
            };

            var paginatedUsers = filteredUsers
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new AdminCreateUserViewModel
            {
                Users = paginatedUsers,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)filteredUsers.Count / PageSize),
                TotalUsers = allUsers.Count,
                TotalAdmins = allUsers.Count(u => u.Role == 0),
                TotalGuests = allUsers.Count(u => u.Role == 1),
            };

            return View(viewModel);
        }

        [Authorize(Roles="2")]
        [HttpGet]
      
        public IActionResult GetFilteredUsers(string filter = "all", int page = 1, string search = "")
        {
            const int PageSize = 8;

            var allUsers = _userService.GetAllUsers().ToList();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                allUsers = allUsers.Where(u => 
                    u.FirstName.ToLower().Contains(search) || 
                    u.LastName.ToLower().Contains(search) || 
                    u.EmailId.ToLower().Contains(search)
                ).ToList();
            }

            // Get filtered users based on filter
            var filteredUsers = filter switch
            {
                "admin" => allUsers.Where(u => u.Role == 0).ToList(),
                "guest" => allUsers.Where(u => u.Role == 1).ToList(),
                _ => allUsers
            };

            // Paginate
            var paginated = PaginationIndexService.Paginate(filteredUsers, page, PageSize);

            var result = new
            {
                Users = paginated.Items.Select(u => new
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
                TotalUsers = allUsers.Count,
                TotalAdmins = allUsers.Count(u => u.Role == 0),
                TotalGuests = allUsers.Count(u => u.Role == 1),
                Filter = filter
            };

            return Json(result);
        }

        [HttpGet]
        [Authorize(Roles="0,1,2")]
        public IActionResult GetUserForEdit(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Json(user);
        }

        [HttpGet]
        [Authorize(Roles="0,1,2")]
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
        [Authorize(Roles="0,1,2")]
        public IActionResult UpdateUser(UserEditViewModel model, IFormFile ProfilePicture, string ExistingProfilePicture, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            // Add debugging information
            _logger.LogInformation($"UpdateUser called - User ID: {model.Id}");
            _logger.LogInformation($"ProfilePicture file: {(ProfilePicture != null ? ProfilePicture.FileName : "null")}");
            _logger.LogInformation($"ExistingProfilePicture: {ExistingProfilePicture}");
            _logger.LogInformation($"Model ProfilePicture: {model.ProfilePicture}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing user to preserve current profile picture if no new file is uploaded
                    var existingUser = _userService.GetUserById(model.Id);
                    if (existingUser == null)
                    {
                        TempData["FormError"] = "User not found.";
                        return RedirectToAction("UserMaster");
                    }

                    _logger.LogInformation($"Existing user profile picture: {existingUser.ProfilePicture}");

                    if (ProfilePicture != null && ProfilePicture.Length > 0)
                    {
                        _logger.LogInformation("Uploading new profile picture to Cloudinary");
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
                        _logger.LogInformation($"New profile picture URL: {model.ProfilePicture}");
                    }
                    else
                    {
                        // Preserve the existing profile picture if no new file is uploaded
                        // Use the hidden input value first, then fall back to the existing user's profile picture
                        model.ProfilePicture = !string.IsNullOrEmpty(ExistingProfilePicture) 
                            ? ExistingProfilePicture 
                            : existingUser.ProfilePicture;
                        _logger.LogInformation($"Preserving existing profile picture: {model.ProfilePicture}");
                    }
                    int? accountId = HttpContext.Session.GetInt32("AccountId");
                    _userService.UpdateUser(model);
               
                    _auditLogService.LogAction("User", "Update", model.Id, accountId.Value, $"{model.FirstName} {model.LastName}" );
                    
                    _logger.LogInformation("User updated successfully");

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("UserMaster");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating user: {ex.Message}");
                    TempData["FormError"] = "Error: " + ex.Message;
                    return RedirectToAction("UserMaster");
                }
            }

            // If model is invalid, redirect with error
            _logger.LogWarning("Model state is invalid");
            TempData["FormError"] = "Validation failed. Please check your inputs.";
            return RedirectToAction("UserMaster");
        }

        [HttpPost]
        [Authorize(Roles="2")]
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

                    int? accountId = HttpContext.Session.GetInt32("AccountId");
                    var newUser = _userService.GetAllUsers()
                        .OrderByDescending(u => u.Id)
                        .FirstOrDefault(u => u.EmailId == model.EmailId);
                    if (newUser != null)
                        _auditLogService.LogAction("User", "Create", newUser.Id, accountId.Value, $"{newUser.FirstName} {newUser.LastName}");
                    

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

        [HttpPost]
        [Authorize(Roles="0,1,2")]
        public IActionResult DeleteUser(int id)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            var user = _userService.GetUserById(id);
            _auditLogService.LogAction("User", "Delete", id, accountId.Value, $"{user.FirstName} {user.LastName}");
            _userService.DeleteUser(id);
          
            
            
            return RedirectToAction("UserMaster");
        }

    }
}
