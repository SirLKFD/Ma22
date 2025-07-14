using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "1")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserService _userService;

        private readonly IAuditLogService _auditLogService;

        public EnrollmentController(IEnrollmentService enrollmentService, IUserService userService, IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
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

            var user = _userService.GetUserByEmailId(userEmail);
            if (user == null)
            {
                return NotFound();  
            }

            _enrollmentService.EnrollUser(user.Id, trainingId);
            _auditLogService.LogAction("User", "Create", trainingId, user.Id, $"Enrolled to {trainingName}");
            TempData["EnrollSuccess"] = true;
            return RedirectToAction("Topics", "UserTopic", new { trainingId = trainingId });
        }
    }
}
