using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly ITrainingCategoryService _trainingCategoryService;
        private readonly ITopicService _topicService;

        public UserController(
            ITrainingService trainingService,
            ITrainingCategoryService trainingCategoryService,
            ITopicService topicService)
        {
            _trainingService = trainingService;
            _trainingCategoryService = trainingCategoryService;
            _topicService = topicService;
        }

        [Authorize(Roles = "1")]
        public IActionResult UserDashboard()
        {
            var trainings = _trainingService.GetAllTrainings();
            return View("UserDashboard", trainings);
        }

        public IActionResult UserTrainings()
        {
            var trainings = _trainingService.GetAllTrainings();
            return View("UserTrainings", trainings);
        }

        public IActionResult BrowseTrainings()
        {
            var categories = _trainingCategoryService.GetAllTrainingCategoryViewModels();
            var trainings = _trainingService.GetAllTrainings();
            var viewModel = new BrowseTrainingsViewModel
            {
                Categories = categories,
                Trainings = trainings
            };
            return View("BrowseTrainings", viewModel);
        }

        public IActionResult UserTopics(int trainingId)
        {
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);
            return View("UserTopics", topics);
        }
    }
}
