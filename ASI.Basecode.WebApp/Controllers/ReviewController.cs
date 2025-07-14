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
        private readonly IAuditLogService _auditLogService;
        private readonly ITrainingService _trainingService;

        public ReviewController(IReviewService reviewService,IAuditLogService auditLogService,ITrainingService trainingService)
        {
            _reviewService = reviewService;
            _auditLogService = auditLogService;
            _trainingService = trainingService;
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
                
                // Check if user has already reviewed this training
                if (_reviewService.HasUserReviewed(accountId.Value, model.TrainingId))
                {
                    TempData["ErrorMessage"] = "You have already reviewed this training.";
                    return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
                }
                
                model.AccountId = accountId.Value;
                try
                {
                    _reviewService.AddReview(model);
                    
                    var accountName = HttpContext.Session.GetString("UserName");
                    var newTraining = _trainingService.GetTrainingById(model.TrainingId);
                    _auditLogService.LogAction("Review", "Create", accountName,model.AccountId,   $"{newTraining.TrainingName}");
            
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
        }

        [HttpPost]
        public IActionResult EditReview(ReviewViewModel model)
        {
            Console.WriteLine($"[ReviewController] Received EditReviewViewModel: " +
            $"ReviewId={model.ReviewId}, Title='{model.Title}', UserReview='{model.UserReview}', ReviewScore={model.ReviewScore}, " +
            $"TrainingId={model.TrainingId}");

            if (ModelState.IsValid)
            {
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                if (accountId == null)
                {
                    Console.WriteLine("❌ AccountId is null");
                    return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
                }

                // Verify the review belongs to the current user
                var existingReview = _reviewService.GetUserReview(accountId.Value, model.TrainingId);
                if (existingReview == null || existingReview.ReviewId != model.ReviewId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own review.";
                    return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
                }

                model.AccountId = accountId.Value;
                try
                {
                    _reviewService.UpdateReview(model);
                    
                    var accountName = HttpContext.Session.GetString("UserName");
                    var training = _trainingService.GetTrainingById(model.TrainingId);
                    _auditLogService.LogAction("Review", "Update", accountName, model.AccountId, $"{training.TrainingName}");
                    
                    TempData["SuccessMessage"] = "Review updated successfully!";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    TempData["ErrorMessage"] = "Failed to update review: " + ex.Message;
                }
            }
            return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
        }

        [HttpGet]
        public IActionResult GetUserReview(int trainingId)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var review = _reviewService.GetUserReview(accountId.Value, trainingId);
            if (review == null)
            {
                return Json(new { success = false, message = "No review found" });
            }

            return Json(new { 
                success = true, 
                review = new {
                    reviewId = review.ReviewId,
                    title = review.Title,
                    userReview = review.UserReview,
                    reviewScore = review.ReviewScore,
                    trainingId = review.TrainingId
                }
            });
        }
    }
}