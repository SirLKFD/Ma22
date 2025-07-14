using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly IEnrollmentService _enrollmentService;
        //private readonly ITopicService _topicService;

        public UserTrainingController(ITrainingService trainingService, IEnrollmentService enrollmentService)
        {
            _trainingService = trainingService;
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public IActionResult Trainings(string search = "", int? categoryId = null)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Account");
            var trainings = _trainingService.GetAllTrainings();
            // Optionally filter by search/category here
            if (!string.IsNullOrWhiteSpace(search))
                trainings = trainings.Where(t => t.TrainingName != null && t.TrainingName.ToLower().Contains(search.ToLower())).ToList();
            if (categoryId.HasValue)
                trainings = trainings.Where(t => t.TrainingCategoryId == categoryId.Value).ToList();
            return View("~/Views/User/_TrainingCardsPartial.cshtml", trainings);
        }

        [HttpGet]
        public IActionResult LoadMoreTrainings(int skip = 9)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Account");
            const int pageSize = 9;
            var trainings = _trainingService.GetAllTrainings();
            var moreTrainings = trainings.Skip(skip).Take(pageSize);
            return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", moreTrainings);
        }

        [HttpGet]
        public IActionResult TrainingsByCategory(int categoryId)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Account");
            IEnumerable<TrainingViewModel> trainings;
            if (categoryId == 0)
            {
                trainings = _trainingService.GetAllTrainings();
            }
            else
            {
                trainings = _trainingService.GetAllTrainings()
                    .Where(t => t.TrainingCategoryId == categoryId).ToList();
            }
                return PartialView("~/Views/User/_TrainingCardsPartial.cshtml", trainings);
        }

        [HttpGet]
        public IActionResult SearchTrainings(string search)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Account");
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
