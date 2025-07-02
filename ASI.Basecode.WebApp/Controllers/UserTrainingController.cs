using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTrainingController : Controller
    {
        private readonly ITrainingService _trainingService;

        public UserTrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet]
        public IActionResult Trainings(string search = "", int? categoryId = null)
        {
            var trainings = _trainingService.GetAllTrainings();
            return View("~/Views/UserTrainings/Trainings.cshtml", trainings);
        }
    }
}
