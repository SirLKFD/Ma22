using ASI.Basecode.Data.Models;
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
using System.Collections.Generic;
using System.Linq;


namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Admin Training Controller
    /// </summary>
    [Route("admin/[action]")]
    [Authorize(Roles = "0,2")]
    public class AdminTrainingController : ControllerBase<AdminTrainingController>
    {
        private readonly ITrainingService _trainingService;
        private readonly ITrainingCategoryService _trainingCategoryService;
        private readonly ITopicService _topicService;
        private readonly IEnrollmentService _enrollmentService;

        private readonly IAuditLogService _auditLogService;

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
                              ITopicService topicService = null,
                              IAuditLogService auditLogService = null,
                              IEnrollmentService enrollmentService = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _trainingService = trainingService;
            _trainingCategoryService = trainingCategoryService;
            _topicService = topicService;
            _enrollmentService = enrollmentService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Returns Admin Training View.
        /// </summary>
        /// <returns> Admin Training View </returns>
        public IActionResult AdminTraining(string search, int? categoryId, int? skillLevelId, int page = 1)
        {
            const int pageSize = 6;
            int totalCount;
            var trainings = _trainingService.GetFilteredTrainings(search, categoryId, skillLevelId, page, pageSize, out totalCount);

            var skillLevels = new Dictionary<int, string>
            {
                { 1, "Beginner" },
                { 2, "Intermediate" },
                { 3, "Advanced" }
            };
            ViewBag.SkillLevels = skillLevels;


            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            ViewData["categories"] = trainingCategories;

            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SelectedCategoryId = categoryId; 
            ViewBag.SelectedSkillLevelId = skillLevelId;

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
                var accountName = HttpContext.Session.GetString("UserName");
                var newTraining = _trainingService.GetAllTrainings().
                OrderByDescending(c=>c.UpdatedTime)
                .FirstOrDefault(c => c.AccountId == model.AccountId && c.TrainingName == model.TrainingName);

                _auditLogService.LogAction("Training", "Create", accountName,model.AccountId,newTraining.TrainingName);

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
                var accountName = HttpContext.Session.GetString("UserName");
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                 _auditLogService.LogAction("Training", "Update", accountName,accountId.Value,model.TrainingName);
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
    
        public IActionResult DeleteTraining(int id, string trainingName)
        {
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            var accountName = HttpContext.Session.GetString("UserName");
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            try{
          

            _auditLogService.LogAction("Training", "Delete", accountName,accountId.Value, trainingName);
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
            var enrollmentCount = _enrollmentService.GetEnrollmentCount(trainingId);
            ViewData["EnrollmentCount"] = enrollmentCount;
            return View("~/Views/Admin/AdminTrainingTopics.cshtml", topics);
        }

        [HttpGet]
        public IActionResult LoadMoreReviews(int trainingId, int skip = 5, int take = 5)
        {
            var training = _trainingService.GetTrainingById(trainingId);
            var reviews = training.Reviews.Skip(skip).Take(take).ToList();
            return PartialView("~/Views/User/_ReviewCardPartial.cshtml", reviews);
        }
    }
}