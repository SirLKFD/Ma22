using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

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

        [HttpGet]
        public IActionResult LoadMoreTrainings(int skip = 9)
        {
            const int pageSize = 9;
            var allTrainings = _trainingService.GetAllTrainings();
            var moreTrainings = allTrainings.Skip(skip).Take(pageSize);
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", moreTrainings);
        }

        [HttpGet]
        public IActionResult TrainingsByCategory(int categoryId)
        {
            var trainings = _trainingService.GetAllTrainingsByCategoryId(categoryId);
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", trainings);
        }

        [HttpGet]
        public IActionResult SearchTrainings(string search)
        {
            var allTrainings = _trainingService.GetAllTrainings();
            var filtered = string.IsNullOrWhiteSpace(search)
                ? allTrainings
                : allTrainings.Where(t => t.TrainingName != null && t.TrainingName.ToLower().Contains(search.ToLower())).ToList();
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", filtered);
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
