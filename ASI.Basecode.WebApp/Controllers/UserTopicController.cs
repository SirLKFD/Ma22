using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTopicController : Controller
    {
        private readonly ITopicService _topicService;

        public UserTopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpGet]
        public IActionResult Topics(int trainingId)
        {
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);
            return View("~/Views/User/UserTopics.cshtml", topics);
        }
    }
}