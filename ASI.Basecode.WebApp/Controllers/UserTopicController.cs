using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly ITrainingService _trainingService;

        public UserTopicController(ITopicService topicService, ITrainingService trainingService)
        {
            _topicService = topicService;
            _trainingService = trainingService;
        }

        [HttpGet]
        public IActionResult Topics(int trainingId)
        {
            var training = _trainingService.GetTrainingById(trainingId);
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);

            ViewData["training"] = training;
            return View("~/Views/User/UserTopics.cshtml", topics);
        }

        [HttpGet]
        public IActionResult TopicDetails(int topicId)
        {
            var topic = _topicService.GetTopicWithAccountById(topicId);
            var allTopics = _topicService.GetAllTopicsByTrainingId(topic.TrainingId);
            ViewBag.AllTopics = allTopics;
            return View("~/Views/User/ViewTopic.cshtml", topic);
        }
    }
}