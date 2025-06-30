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
        public IActionResult AdminTrainingCategory()
        {
            List<TrainingCategoryViewModel> categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();

            return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult AddTrainingCategory(
            TrainingCategoryViewModel model,
            IFormFile CoverPictureAdd,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            List<TrainingCategoryViewModel> categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
            }

            try
            {
                if (CoverPictureAdd != null && CoverPictureAdd.Length > 0)
                {
                    using var stream = CoverPictureAdd.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPictureAdd.FileName, stream),
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

                _trainingCategoryService.AddTrainingCategory(model);
                Console.WriteLine("✅ Training category added");

                ViewBag.NewCategory = model;

                return RedirectToAction("AdminTrainingCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
            }
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult EditTrainingCategory(TrainingCategoryViewModel model, IFormFile CoverPictureEdit, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            List<TrainingCategoryViewModel> categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);  
            }

            var existingCategory = _trainingCategoryService.GetTrainingCategoryById(model.Id);
            model.AccountId = existingCategory.AccountId;
            model.AccountFirstName = existingCategory.AccountFirstName;
            model.AccountLastName = existingCategory.AccountLastName;

            Console.WriteLine($"CoverPicture is null: {CoverPictureEdit == null}, Length: {CoverPictureEdit?.Length}");
            try
            {
                if (CoverPictureEdit != null && CoverPictureEdit.Length > 0)
                {
                    using var stream = CoverPictureEdit.OpenReadStream();

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(CoverPictureEdit.FileName, stream),
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
                else{
                    model.CoverPicture = existingCategory.CoverPicture;
                }

                _trainingCategoryService.EditTrainingCategory(model);
                Console.WriteLine("✅ Training category edited");

                ViewBag.EditedCategory = model;

                return RedirectToAction("AdminTrainingCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
            }
        }

        [HttpPost]
        [Authorize(Roles = "0")]
        public IActionResult DeleteTrainingCategory(int id)
        {
            List<TrainingCategoryViewModel> categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            try{
            _trainingCategoryService.DeleteTrainingCategory(id);
            return RedirectToAction("AdminTrainingCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
            }
        }

        [HttpGet]
        public IActionResult GetTrainingCategory(int id)
        {
            var category = _trainingCategoryService.GetTrainingCategoryById(id);
            return View("~/Views/Admin/AdminEditTrainingCategory.cshtml", category);
        }
    }
}