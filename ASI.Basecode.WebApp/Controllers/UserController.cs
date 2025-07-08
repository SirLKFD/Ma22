using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ITrainingCategoryService _trainingCategoryService;
        private readonly ITopicService _topicService;
        
        private readonly IUserService _userService;

        private readonly ILogger<UserController> _logger;

        private readonly IEnrollmentService _enrollmentService;  


        public UserController(
            ITrainingService trainingService,
            ITrainingCategoryService trainingCategoryService,
            ITopicService topicService,
            IUserService userService,
            IEnrollmentService enrollmentService,
            ILogger<UserController> logger)
        {
            _trainingService = trainingService;
            _trainingCategoryService = trainingCategoryService;
            _topicService = topicService;
            _userService = userService;
            _enrollmentService = enrollmentService;
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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userService.GetUserByEmailId(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var trainings = _enrollmentService.GetEnrolledTrainings(user.Id);
            var enrolledCategoryIds = trainings.Select(t => t.TrainingCategoryId).Distinct().ToList();
            var categories = _trainingCategoryService.GetTrainingCategoryViewModelsByIds(enrolledCategoryIds);

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
            var categories = _trainingCategoryService.GetAllTrainingCategoryViewModelsUnfiltered();
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
        

       
        [HttpPost]
        public IActionResult DeleteUser(int idToDelete)
        {
              if (!ModelState.IsValid)
            {
                Console.WriteLine($"Failed to Delete {idToDelete}");
                return RedirectToAction("UserProfile");
            }

            _userService.DeleteUser(idToDelete);
           
            HttpContext.Session.Clear();

            return RedirectToAction("SignOutUser","Account");
        }

        [HttpGet]
        public IActionResult UserTrainingsByCategory(int categoryId)
        {
            var trainings = _trainingService.GetAllTrainingsByCategoryId(categoryId);
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", trainings);
        }

        [HttpGet]
        public IActionResult SearchUserTrainings(string search)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }
            var user = _userService.GetUserByEmailId(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            var enrolledTrainings = _enrollmentService.GetEnrolledTrainings(user.Id);
            var filtered = string.IsNullOrWhiteSpace(search)
                ? enrolledTrainings
                : enrolledTrainings.Where(t => t.TrainingName != null && t.TrainingName.ToLower().Contains(search.ToLower())).ToList();
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", filtered);
        }

        [HttpGet]
        public IActionResult LoadMoreUserTrainings(int skip = 9)
        {
            const int pageSize = 9;
            var allTrainings = _trainingService.GetAllTrainings();
            var moreTrainings = allTrainings.Skip(skip).Take(pageSize);
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", moreTrainings);
        }
    }
}
