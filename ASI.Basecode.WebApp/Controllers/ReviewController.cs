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
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public IActionResult AddReview(ReviewViewModel model)
        {

             Console.WriteLine($"[ReviewController] Received ReviewViewModel: " +
            $"Title='{model.Title}', UserReview='{model.UserReview}', ReviewScore={model.ReviewScore}, " +
            $"TrainingId={model.TrainingId}");

            if (ModelState.IsValid)
            {
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                if (accountId == null)
                {
                    Console.WriteLine("❌ AccountId is null");
                    return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
                }
                model.AccountId = accountId.Value;
                try
                {
                    _reviewService.AddReview(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
        }
    }
}