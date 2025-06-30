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
        public IActionResult AdminTraining()
        {
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();

            Console.WriteLine($"Found {trainings?.Count ?? 0} trainings"); 
            
            ViewData["categories"] = trainingCategories;
            
            return View("~/Views/Admin/AdminTraining.cshtml", trainings);
        }

        [HttpPost]
        public IActionResult AddTraining(
            TrainingViewModel model,
            IFormFile CoverPicture,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            Console.WriteLine($"TrainingCategoryId submitted: {model.TrainingCategoryId}");
            Console.WriteLine($"SkillLevel submitted: {model.SkillLevel}");
            
            List<TrainingViewModel> trainings = _trainingService.GetAllTrainings();
            List<TrainingCategoryViewModel> trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            ViewData["categories"] = trainingCategories;
            
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }

            try
            {
                if (CoverPicture != null && CoverPicture.Length > 0)
                {
                    using var stream = CoverPicture.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPicture.FileName, stream),
                        Folder = "training_category"
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
                
                // Get the actual text values for SkillLevel and Duration
                var skillLevelText = GetSkillLevelText(model.SkillLevel);
                var durationText = GetDurationText(model.Duration);
                var trainingCategoryText = GetTrainingCategoryText(model.TrainingCategoryId);
                
                ViewBag.NewTraining = model;
                ViewBag.SkillLevelText = skillLevelText;
                ViewBag.DurationText = durationText;
                ViewBag.TrainingCategoryText = trainingCategoryText;
                
                return RedirectToAction("AdminTraining");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTraining.cshtml", trainings);
            }
        }

        private string GetSkillLevelText(int skillLevelId)
        {
            return skillLevelId switch
            {
                1 => "Beginner",
                2 => "Intermediate", 
                3 => "Advanced",
                _ => "Unknown"
            };
        }

        private string GetDurationText(int? durationId)
        {
            return durationId switch
            {
                1 => "1 week",
                2 => "2 weeks",
                3 => "3 weeks",
                4 => "1 month",
                5 => "2 months",
                6 => "3 months",
                7 => "4–6 months",
                8 => "6–9 months",
                9 => "9–12 months",
                10 => "1 year",
                11 => "More than 1 year",
                _ => "Unknown"
            };
        }

        private string GetTrainingCategoryText(int trainingCategoryId)
        {
            var trainingCategory = _trainingCategoryService.GetTrainingCategoryById(trainingCategoryId);
            return trainingCategory?.CategoryName ?? "Unknown";
        }
        [HttpGet]
        public IActionResult AdminTrainingTopics(int trainingId)
        {
            
            var training = _trainingService.GetTrainingById(trainingId); 
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);
            ViewBag.Training = training;
            ViewBag.Topics = topics;
            return View("~/Views/Admin/AdminTrainingTopics.cshtml");
        }
    }
}