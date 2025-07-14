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
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(ReviewViewModel model)
        {

             Console.WriteLine($"[ReviewController] Received ReviewViewModel: " +
            $"Title='{model.Title}', UserReview='{model.UserReview}', ReviewScore={model.ReviewScore}, " +
            $"TrainingId={model.TrainingId}");

            // Check if anti-forgery token is present
            var hasAntiForgeryToken = Request.Form.ContainsKey("__RequestVerificationToken");
            Console.WriteLine($"🔐 Anti-forgery token present: {hasAntiForgeryToken}");

            // Log model binding state
            Console.WriteLine($"📋 Model binding state:");
            Console.WriteLine($"   - Model is null: {model == null}");
            if (model != null)
            {
                Console.WriteLine($"   - Title: '{model.Title}' (null: {model.Title == null})");
                Console.WriteLine($"   - UserReview: '{model.UserReview}' (null: {model.UserReview == null})");
                Console.WriteLine($"   - ReviewScore: {model.ReviewScore}");
                Console.WriteLine($"   - TrainingId: {model.TrainingId}");
                Console.WriteLine($"   - AccountId: {model.AccountId}");
            }

            if (ModelState.IsValid)
            {
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                if (accountId == null)
                {
                    Console.WriteLine("❌ AccountId is null");
                    TempData["ErrorMessage"] = "You must be logged in to submit a review.";
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
                    
                    TempData["SuccessMessage"] = "Review submitted successfully!";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Exception in AddReview: {ex.Message}");
                    ModelState.AddModelError("", ex.Message);
                    TempData["ErrorMessage"] = "Failed to submit review: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState is invalid");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"❌ Validation errors: {string.Join(", ", errors)}");
                
                // Log each ModelState entry for debugging
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (!entry.ValidationState.ToString().Contains("Valid"))
                    {
                        Console.WriteLine($"❌ ModelState[{key}]: {entry.ValidationState} - {string.Join(", ", entry.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors: " + string.Join(", ", errors);
            }
            return RedirectToAction("Topics", "UserTopic", new { trainingId = model.TrainingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    TempData["ErrorMessage"] = "You must be logged in to edit a review.";
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
                    Console.WriteLine($"❌ Exception in EditReview: {ex.Message}");
                    ModelState.AddModelError("", ex.Message);
                    TempData["ErrorMessage"] = "Failed to update review: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState is invalid");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"❌ Validation errors: {string.Join(", ", errors)}");
                
                // Log each ModelState entry for debugging
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (!entry.ValidationState.ToString().Contains("Valid"))
                    {
                        Console.WriteLine($"❌ ModelState[{key}]: {entry.ValidationState} - {string.Join(", ", entry.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                TempData["ErrorMessage"] = "Please correct the following errors: " + string.Join(", ", errors);
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