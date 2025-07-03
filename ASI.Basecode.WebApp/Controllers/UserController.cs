using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ITrainingCategoryService _trainingCategoryService;
        private readonly ITopicService _topicService;
        
        private readonly IUserService _userService;

        private readonly ILogger<UserController> _logger;

        public UserController(
            ITrainingService trainingService,
            ITrainingCategoryService trainingCategoryService,
            ITopicService topicService,
            IUserService userService,
            ILogger<UserController> logger)
        {
            _trainingService = trainingService;
            _trainingCategoryService = trainingCategoryService;
            _topicService = topicService;
            _userService = userService;
             _logger = logger;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public IActionResult UserDashboard()
        {
            var trainings = _trainingService.GetAllTrainings();
            return View("UserDashboard", trainings);
        }

        [HttpGet]
        public IActionResult UserTrainings()
        {
            var categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            var trainings = _trainingService.GetAllTrainings();
            var viewModel = new BrowseTrainingsViewModel
            {
                Categories = categories,
                Trainings = trainings
            };
            return View("UserTrainings", viewModel);
        }

        [HttpGet]
        public IActionResult BrowseTrainings()
        {
            var categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            var trainings = _trainingService.GetAllTrainings();
            var viewModel = new BrowseTrainingsViewModel
            {
                Categories = categories,
                Trainings = trainings
            };
            return View("BrowseTrainings", viewModel);
        }

        [HttpGet]
        public IActionResult UserTopics(int trainingId)
        {
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);
            return View("UserTopics", topics);
        }

        [HttpGet]
        public IActionResult UserProfile(){
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _userService.GetUserByEmailId(userEmail);
            return View("UserProfile", user);
        }

        [HttpPost]
        public IActionResult UserProfileEdit(UserProfileEditViewModel model, IFormFile ProfilePictureFile, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            // Add debugging information
            _logger.LogInformation($"UpdateUser called - User ID: {model.Id}");
            _logger.LogInformation($"ProfilePicture file: {(ProfilePictureFile != null ? ProfilePictureFile.FileName : "null")}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing user to preserve current profile picture if no new file is uploaded
                    var existingUser = _userService.GetUserById(model.Id);
                    if (existingUser == null)
                    {
                        TempData["FormError"] = "User not found.";
                        return RedirectToAction("UserProfile");
                    }

                    _logger.LogInformation($"Existing user profile picture: {existingUser.ProfilePicture}");

                    if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
                    {
                        _logger.LogInformation("Uploading new profile picture to Cloudinary");
                        using var stream = ProfilePictureFile.OpenReadStream();
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(ProfilePictureFile.FileName, stream),
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
                        model.ProfilePicture = existingUser.ProfilePicture;
                        _logger.LogInformation($"Preserving existing profile picture: {model.ProfilePicture}");
                    }

                    _userService.UpdateUser(model);
                    _logger.LogInformation("User updated successfully");

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("UserProfile");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating user: {ex.Message}");
                    TempData["FormError"] = "Error: " + ex.Message;
                    return RedirectToAction("UserProfile");
                }
            }

            // If model is invalid, redirect with error
            _logger.LogWarning("Model state is invalid");
            TempData["FormError"] = "Validation failed. Please check your inputs.";
            return RedirectToAction("UserProfile");
        }
    }
}
