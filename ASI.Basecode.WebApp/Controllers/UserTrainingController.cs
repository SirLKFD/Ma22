using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        //private readonly ITopicService _topicService;

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

        //public IActionResult UserTrainingTopics(int trainingId)
        //{

        //    var training = _trainingService.GetTrainingById(trainingId);
        //    var topics = _topicService.GetAllTopicsByTrainingId(trainingId);

        //    ViewData["training"] = training;

        //    return View("~/Views/User/UserTopics.cshtml", topics);
        //}
    }
}
