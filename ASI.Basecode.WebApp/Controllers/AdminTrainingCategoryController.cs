using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ASI.Basecode.Services.ServiceModels;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("admin/[action]")]
    public class AdminTrainingCategoryController : ControllerBase<AdminTrainingCategoryController>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="localizer"></param>
        /// <param name="mapper"></param>
        private readonly ITrainingCategoryService _trainingCategoryService;

        
        public AdminTrainingCategoryController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null,
                              ITrainingCategoryService trainingCategoryService = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _trainingCategoryService = trainingCategoryService;
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>
        /// 
        
        [Authorize(Roles = "0")]
        public IActionResult TrainingCategory()
        {
            return View("~/Views/Admin/AdminTrainingCategory.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult AddTrainingCategory(
            TrainingCategoryViewModel model,
            IFormFile CoverPicture,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", model);
            }

            try
            {
                if (CoverPicture != null && CoverPicture.Length > 0)
                {
                    using var stream = CoverPicture.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPicture.FileName, stream),
                        Folder = "user_profiles"
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

                _trainingCategoryService.AddTrainingCategory(model);
                Console.WriteLine("✅ Training category added");
                ViewBag.NewCategory = model;
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", model);
            }
        }
    }
}