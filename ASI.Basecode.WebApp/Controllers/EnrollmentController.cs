using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserService _userService;

         private readonly ITrainingService _trainingService;
        private readonly IAuditLogService _auditLogService;

        public EnrollmentController(IEnrollmentService enrollmentService, ITrainingService trainingService, IUserService userService, IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
            _trainingService = trainingService;
            _enrollmentService = enrollmentService;
            _userService = userService;
        }

        [HttpPost]
        public IActionResult Enroll(int trainingId, string trainingName)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();  
            }
            var accountName = HttpContext.Session.GetString("UserName");
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            var user = _userService.GetUserByEmailId(userEmail);
            if (user == null)
            {
                return NotFound();  
            }

            _enrollmentService.EnrollUser(user.Id, trainingId);
            _auditLogService.LogAction("Enroll", "Create", accountName, accountId.Value, $"{trainingName}");
            TempData["EnrollSuccess"] = true;
            return RedirectToAction("Topics", "UserTopic", new { trainingId = trainingId });
        }

            [HttpGet]
          [Route("admin/[action]")]
          public IActionResult ManageEnrollments(int trainingId)
        {
            try
            {
                var training = _trainingService.GetTrainingById(trainingId);
                var enrollees = _enrollmentService.GetEnrolleesForTraining(trainingId);
                
                ViewData["Training"] = training;
                ViewData["Enrollees"] = enrollees;
                
                return View("~/Views/Admin/ManageEnrollments.cshtml", enrollees);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load enrollments: " + ex.Message;
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = trainingId });
            }
        }

        [HttpPost]
        [Route("admin/[action]")]
        public IActionResult DeleteEnrollment(int enrollmentId, int trainingId)
        {
            try
            {
                var enrollment = _enrollmentService.GetEnrolleesForTraining(trainingId)
                    .FirstOrDefault(e => e.EnrollmentId == enrollmentId);
                
                if (enrollment == null)
                {
                    TempData["ErrorMessage"] = "Enrollment not found.";
                    return RedirectToAction("ManageEnrollments", new { trainingId = trainingId });
                }

                _enrollmentService.DeleteEnrollment(enrollmentId);
                
                var accountName = HttpContext.Session.GetString("UserName");
                var training = _trainingService.GetTrainingById(trainingId);
                _auditLogService.LogAction("Enrollment", "Delete", accountName, enrollment.AccountId, $"Removed {enrollment.FullName} from {training.TrainingName}");
                
                TempData["SuccessMessage"] = $"Successfully removed {enrollment.FullName} from the training.";
                return RedirectToAction("ManageEnrollments", new { trainingId = trainingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to delete enrollment: " + ex.Message;
                return RedirectToAction("ManageEnrollments", new { trainingId = trainingId });
            }
        }
    }
}

   