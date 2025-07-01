using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.ServiceModels;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using System;
using ASI.Basecode.Services.Interfaces;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;


namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Admin Training Controller
    /// </summary>
    [Route("admin/[action]")]
    [Authorize(Roles = "0")]
    public class AdminTrainingController : ControllerBase<AdminTrainingController>
    {
        private readonly ITrainingService _trainingService;
        private readonly ITrainingCategoryService _trainingCategoryService;
        private readonly ITopicService _topicService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        public AdminTrainingController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null,
                              ITrainingService trainingService = null,
                              ITrainingCategoryService trainingCategoryService = null,
                              ITopicService topicService = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _trainingService = trainingService;
            _trainingCategoryService = trainingCategoryService;
            _topicService = topicService;
        }

        /// <summary>
        /// Returns Admin Training View.
        /// </summary>
        /// <returns> Admin Training View </returns>
        public IActionResult AdminTraining(string search, int page = 1)
        {
            const int pageSize = 6;
            int totalCount;
            var trainings = _trainingService.GetPaginatedTrainings(search, page, pageSize, out totalCount);
            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            ViewData["categories"] = trainingCategories;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            return View("~/Views/Admin/AdminTraining.cshtml", trainings);
        }

        [HttpPost]
        public IActionResult AddTraining(
            TrainingViewModel model,
            IFormFile CoverPictureAddTraining,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            ViewData["categories"] = trainingCategories;
            
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }

            try
            {
                if (CoverPictureAddTraining != null && CoverPictureAddTraining.Length > 0)
                {
                    using var stream = CoverPictureAddTraining.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPictureAddTraining.FileName, stream),
                        Folder = "training"
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        Console.WriteLine("❌ Upload failed: " + uploadResult.Error.Message);
                        throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                    }
                    else
                    {
                        Console.WriteLine("✅ Upload succeeded");
                        Console.WriteLine("Secure URL: " + uploadResult.SecureUrl);

                        model.CoverPicture = uploadResult.SecureUrl?.ToString();
                    }
                }

                int? accountId = HttpContext.Session.GetInt32("AccountId");
                if (accountId == null)
                {
                    Console.WriteLine("❌ AccountId is null");
                    // Handle missing AccountId
                    return RedirectToAction("Login", "Account");
                }
                model.AccountId = accountId.Value;
                _trainingService.AddTraining(model);
                Console.WriteLine("✅ Training added");

                return RedirectToAction("AdminTraining");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }
        }

        [HttpPost]
        public IActionResult UpdateTraining(
            TrainingViewModel model,
            IFormFile CoverPictureEditTraining,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            _logger.LogInformation("[UpdateTraining] Called with model.Id={Id}", model.Id);
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            ViewData["categories"] = trainingCategories;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[UpdateTraining] ModelState is invalid for model.Id={Id}", model.Id);
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }

            var existingTraining = _trainingService.GetTrainingById(model.Id);
            if (existingTraining != null)
            {
                _logger.LogInformation("[UpdateTraining] Found existing training for Id={Id}", model.Id);
                model.AccountId = existingTraining.AccountId;
                model.AccountFirstName = existingTraining.AccountFirstName;
                model.AccountLastName = existingTraining.AccountLastName;
            }
            else
            {
                _logger.LogWarning("[UpdateTraining] No existing training found for Id={Id}", model.Id);
            }

            try
            {
                if (CoverPictureEditTraining != null && CoverPictureEditTraining.Length > 0)
                {
                    _logger.LogInformation("[UpdateTraining] Uploading new cover picture for Id={Id}", model.Id);
                    using var stream = CoverPictureEditTraining.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPictureEditTraining.FileName, stream),
                        Folder = "training"
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogError("[UpdateTraining] Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                        throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                    }
                    else
                    {
                        _logger.LogInformation("[UpdateTraining] Cloudinary upload succeeded. Secure URL: {Url}", uploadResult.SecureUrl);
                        model.CoverPicture = uploadResult.SecureUrl?.ToString();
                    }
                }
                else
                {
                    _logger.LogInformation("[UpdateTraining] No new cover picture uploaded for Id={Id}. Using existing.", model.Id);
                    model.CoverPicture = existingTraining.CoverPicture;
                }

                _logger.LogInformation("[UpdateTraining] Calling _trainingService.UpdateTraining for Id={Id}", model.Id);
                _trainingService.UpdateTraining(model);
                _logger.LogInformation("[UpdateTraining] Training updated successfully for Id={Id}", model.Id);

                return RedirectToAction("AdminTraining");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateTraining] Exception occurred for Id={Id}", model.Id);
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult DeleteTraining(int id)
        {
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            try{
            _trainingService.DeleteTraining(id);
            return RedirectToAction("AdminTraining");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }
        }

        [HttpGet]
        public IActionResult AdminTrainingTopics(int trainingId)
        {
            
            var training = _trainingService.GetTrainingById(trainingId); 
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);

            ViewData["training"] = training;

            return View("~/Views/Admin/AdminTrainingTopics.cshtml", topics);
        }
    }
}