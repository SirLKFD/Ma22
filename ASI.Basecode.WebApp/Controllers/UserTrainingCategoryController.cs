using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTrainingCategoryController : Controller
    {
        private readonly ITrainingCategoryService _trainingCategoryService;

        public UserTrainingCategoryController(ITrainingCategoryService trainingCategoryService)
        {
            _trainingCategoryService = trainingCategoryService;
        }

        [HttpGet]
        public IActionResult Categories()
        {
            var categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            return View("~/Views/UserTrainingCategory/Categories.cshtml", categories);
        }
    }
}
