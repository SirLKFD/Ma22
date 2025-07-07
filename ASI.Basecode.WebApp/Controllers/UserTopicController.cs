using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserTopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly ITrainingService _trainingService;
        private readonly IUserService _userService;
        private readonly IEnrollmentService _enrollmentService;


        public UserTopicController(
            ITopicService topicService,
            ITrainingService trainingService,
            IUserService userService,
            IEnrollmentService enrollmentService)
        {
            _topicService = topicService;
            _trainingService = trainingService;
            _userService = userService;
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public IActionResult Topics(int trainingId)
        {
            var training = _trainingService.GetTrainingById(trainingId);
            var topics = _topicService.GetAllTopicsByTrainingId(trainingId);

            ViewData["training"] = training;

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = _userService.GetUserByEmailId(userEmail);
                if (user != null)
                {
                    bool isEnrolled = _enrollmentService.IsUserEnrolled(user.Id, trainingId);
                    ViewBag.IsEnrolled = isEnrolled;
                }
            }

            var enrollmentCount = _enrollmentService.GetEnrollmentCount(trainingId);
            ViewData["EnrollmentCount"] = enrollmentCount;

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