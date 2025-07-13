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
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("admin/[action]")]
    [Authorize(Roles = "0,2")]
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
        private readonly ITrainingService _trainingService;
        private readonly IAuditLogService _auditLogService;

        
        public AdminTrainingCategoryController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null,
                              ITrainingCategoryService trainingCategoryService = null,
                              IAuditLogService auditLogService = null,
                              ITrainingService trainingService = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _trainingCategoryService = trainingCategoryService;
            _trainingService = trainingService;
             _auditLogService = auditLogService; 
        }

        /// <summary>
        /// Returns Home View.
        /// </summary>
        /// <returns> Home View </returns>
        /// 
        
    
        public IActionResult AdminTrainingCategory(string search, int page = 1)
        {
            const int pageSize = 6;
            int totalCount;


            var categories = _trainingCategoryService.GetPaginatedTrainingCategories(search, page, pageSize, out totalCount);

            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);
        }

        [HttpPost]
 
        public IActionResult AddTrainingCategory(
            TrainingCategoryViewModel model,
            IFormFile CoverPictureAdd,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            int totalCount;
            const int pageSize = 6;
            int currentPage = 1;
            string search = "";

            var categories = _trainingCategoryService.GetPaginatedTrainingCategories(search, currentPage, pageSize, out totalCount);
            
            if (!ModelState.IsValid)
            {
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = currentPage;
                ViewBag.Search = search;
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
                var accountName = HttpContext.Session.GetString("UserName");
                var newCategory = _trainingCategoryService
                    .GetAllTrainingCategories()
                    .OrderByDescending(c => c.CreatedTime)
                    .FirstOrDefault(c => c.AccountId == model.AccountId && c.CategoryName == model.CategoryName);


                _auditLogService.LogAction("TrainingCategory", "Create", accountName,model.AccountId, newCategory.CategoryName);

                ViewBag.NewCategory = model;

                return RedirectToAction("AdminTrainingCategory", new { page = 1, search = "" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = currentPage;
                ViewBag.Search = search;
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);

            }
        }

        [HttpPost]
      
        public IActionResult EditTrainingCategory(TrainingCategoryViewModel model, IFormFile CoverPictureEdit, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            int totalCount;
            const int pageSize = 6;
            int currentPage = 1;
            string search = "";

            var categories = _trainingCategoryService.GetPaginatedTrainingCategories(search, currentPage, pageSize, out totalCount);

            if (!ModelState.IsValid)
            {
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = currentPage;
                ViewBag.Search = search;
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
                var accountName = HttpContext.Session.GetString("UserName");
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                _trainingCategoryService.EditTrainingCategory(model);
                _auditLogService.LogAction("TrainingCategory", "Update", accountName,accountId.Value,model.CategoryName);
                Console.WriteLine("✅ Training category edited");

                ViewBag.EditedCategory = model;

                return RedirectToAction("AdminTrainingCategory", new { page = 1, search = "" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = currentPage;
                ViewBag.Search = search;
                return View("~/Views/Admin/AdminTrainingCategory.cshtml", categories);

            }
        }

        [HttpPost]
      
        public IActionResult DeleteTrainingCategory(int id, string categoryName)
        {
            int totalCount;
            const int pageSize = 6;
            int currentPage = 1;
            string search = "";
           
            var accountName = HttpContext.Session.GetString("UserName");
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            var categories = _trainingCategoryService.GetPaginatedTrainingCategories(search, currentPage, pageSize, out totalCount);
            try
            {
            _auditLogService.LogAction("TrainingCategory", "Delete", accountName,accountId.Value, categoryName);
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
        public IActionResult TrainingCategoryDetails(int categoryId, string search = "", int page = 1)
        {
            const int pageSize = 6;
            int totalCount;
            // Get paginated and filtered trainings by category
            var trainings = string.IsNullOrEmpty(search)
                ? _trainingService.GetAllTrainingsByCategoryId(categoryId)
                : _trainingService.GetAllTrainingsByCategoryId(categoryId).FindAll(t => t.TrainingName.Contains(search, StringComparison.OrdinalIgnoreCase));
            // Manual pagination for filtered results
            totalCount = trainings.Count;
            var pagedTrainings = trainings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var trainingCategories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            var category = trainingCategories.Find(c => c.Id == categoryId);
            if (category != null)
            {
                ViewBag.CategoryName = category.CategoryName;
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewData["categories"] = trainingCategories;
            return View("~/Views/Admin/AdminTraining.cshtml", pagedTrainings);
        }
    }
}