using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = _userService.GetUserByEmailId(userEmail);
                if (user != null)
                {
                    bool isEnrolled = _enrollmentService.IsUserEnrolled(user.Id, topic.TrainingId);
                    ViewBag.IsEnrolled = isEnrolled;
                }
                else
                {
                    ViewBag.IsEnrolled = false;
                }
            }
            else
            {
                ViewBag.IsEnrolled = false;
            }
            return View("~/Views/User/ViewTopic.cshtml", topic);
        }

        [HttpGet]
        public IActionResult LoadMoreReviews(int trainingId, int skip = 5, int take = 5)
        {
            var training = _trainingService.GetTrainingById(trainingId);
            var reviews = training.Reviews.Skip(skip).Take(take).ToList();
            return PartialView("~/Views/User/_ReviewCardPartial.cshtml", reviews);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMediaZip(int topicId, string mode, List<int>? selectedMediaIds = null, string topicTitle = null)
        {
            try
            {
                var topic = _topicService.GetTopicWithAccountById(topicId);
                var mediaList = topic?.Media ?? new List<TopicMediaViewModel>();

                if (mode == "selected" && selectedMediaIds != null)
                {
                    mediaList = mediaList.Where(m => selectedMediaIds.Contains(m.Id)).ToList();
                }
                else if (mode == "allmedia" || mode == "mediaonly")
                {
                    mediaList = mediaList.Where(m =>
                        IsVideoFile(m.MediaType) ||
                        IsDocumentFile(m.MediaType, m.Name) ||
                        IsImageFile(m.MediaType)).ToList();
                }
                else if (mode == "videos")
                {
                    mediaList = mediaList.Where(m => IsVideoFile(m.MediaType)).ToList();
                }
                else if (mode == "documents")
                {
                    mediaList = mediaList.Where(m => IsDocumentFile(m.MediaType, m.Name)).ToList();
                }
                else if (mode == "images")
                {
                    mediaList = mediaList.Where(m => IsImageFile(m.MediaType)).ToList();
                }

                using var memoryStream = new System.IO.MemoryStream();
                using (var zip = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var media in mediaList)
                    {
                        if (string.IsNullOrWhiteSpace(media.MediaUrl))
                            continue;

                        try
                        {
                            using var client = new System.Net.Http.HttpClient();
                            var fileData = await client.GetByteArrayAsync(media.MediaUrl);
                            var entry = zip.CreateEntry(media.Name ?? $"media_{media.Id}");

                            using var entryStream = entry.Open();
                            await entryStream.WriteAsync(fileData, 0, fileData.Length);
                        }
                        catch (System.Exception ex)
                        {
                            System.Console.WriteLine($"[DownloadMediaZip] Skipped media {media.Id}: {ex.Message}");
                        }
                    }
                }

                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

                // Sanitize topicTitle for file name
                string safeTitle = string.IsNullOrWhiteSpace(topicTitle) ? $"Topic_{topicId}" : string.Join("_", topicTitle.Split(System.IO.Path.GetInvalidFileNameChars()));
                string fileName = "";
                if (mode == "all" || mode == "allmedia")
                {
                    fileName = $"{safeTitle}.zip";
                }
                else if (mode == "videos")
                {
                    fileName = $"{safeTitle}-Video.zip";
                }
                else if (mode == "documents")
                {
                    fileName = $"{safeTitle}-Documents.zip";
                }
                else if (mode == "images")
                {
                    fileName = $"{safeTitle}-Pictures.zip";
                }
                else if (mode == "mediaonly")
                {
                    if (mediaList.All(m => IsVideoFile(m.MediaType)))
                        fileName = $"{safeTitle}-Video.zip";
                    else if (mediaList.All(m => IsDocumentFile(m.MediaType, m.Name)))
                        fileName = $"{safeTitle}-Documents.zip";
                    else if (mediaList.All(m => IsImageFile(m.MediaType)))
                        fileName = $"{safeTitle}-Pictures.zip";
                    else
                        fileName = $"{safeTitle}-Media.zip";
                }
                else if (mode == "selected")
                {
                    if (mediaList.Count == 1)
                    {
                        var media = mediaList.First();
                        using var client = new System.Net.Http.HttpClient();
                        var fileData = await client.GetByteArrayAsync(media.MediaUrl);
                        string singleFileName = media.Name ?? $"media_{media.Id}";
                        return File(fileData, "application/octet-stream", singleFileName);
                    }
                    else
                    {
                        fileName = $"{safeTitle}-Content.zip";
                    }
                }
                else
                {
                    fileName = $"{safeTitle}-Media.zip";
                }

                return File(memoryStream.ToArray(), "application/zip", fileName);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[DownloadMediaZip] Exception: {ex.Message}");
                TempData["Error"] = "Failed to download ZIP: " + ex.Message;
                return RedirectToAction("TopicDetails", new { topicId });
            }
        }

        // Helper methods for file type checks
        private bool IsVideoFile(string mediaType)
        {
            return mediaType != null && mediaType.ToLower().Contains("video");
        }

        private bool IsImageFile(string mediaType)
        {
            return mediaType != null && mediaType.ToLower().Contains("image");
        }

        private bool IsDocumentFile(string mediaType, string fileName)
        {
            if (mediaType != null)
            {
                var type = mediaType.ToLower();
                if (type.Contains("pdf") ||
                    type.Contains("word") ||
                    type.Contains("excel") ||
                    type.Contains("powerpoint") ||
                    type.Contains("spreadsheet") ||
                    type.Contains("presentation") ||
                    type.Contains("msword") ||
                    type.Contains("vnd.openxmlformats") ||
                    type.Contains("vnd.ms-"))
                {
                    return true;
                }
            }
            if (fileName != null)
            {
                var ext = fileName.ToLower();
                return ext.EndsWith(".pdf") ||
                       ext.EndsWith(".doc") ||
                       ext.EndsWith(".docx") ||
                       ext.EndsWith(".xls") ||
                       ext.EndsWith(".xlsx") ||
                       ext.EndsWith(".ppt") ||
                       ext.EndsWith(".pptx");
            }
            return false;
        }
    }
}