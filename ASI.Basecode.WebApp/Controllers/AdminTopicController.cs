using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("admin/[action]")]
    public class AdminTopicController : ControllerBase<AdminTopicController>
    {
        private readonly ITopicService _topicService;
        private readonly ITopicMediaService _topicMediaService;
        private readonly IUserService _userService;
        public AdminTopicController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper = null,
            ITopicService topicService = null,
            ITopicMediaService topicMediaService = null,
            IUserService userService = null
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _topicService = topicService;
            _topicMediaService = topicMediaService;
            _userService = userService;
        }

        [HttpPost]
        public IActionResult AddTopic(
            TopicViewModel model,
            List<IFormFile> VideoFiles,
            List<IFormFile> ImageFiles,
            List<IFormFile> DocumentFiles,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }

            try
            {
               
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                if (accountId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                model.AccountId = accountId.Value;

                _topicService.AddTopic(model);

                var addedTopic = _topicService.GetAllTopicsByTrainingId(model.TrainingId)
                    .OrderByDescending(t => t.CreatedTime).FirstOrDefault(t => t.TopicName == model.TopicName);
                if (addedTopic != null)
                {
                    Console.WriteLine($"[AddTopic] Fetched added topic with ID: {addedTopic.Id}");
                }
                else
                {
                    Console.WriteLine("[AddTopic] Could not fetch the newly added topic.");
                }

                var allFiles = new List<IFormFile>();
                if (VideoFiles != null) { allFiles.AddRange(VideoFiles); Console.WriteLine($"[AddTopic] {VideoFiles.Count} video files received."); }
                if (ImageFiles != null) { allFiles.AddRange(ImageFiles); Console.WriteLine($"[AddTopic] {ImageFiles.Count} image files received."); }
                if (DocumentFiles != null) { allFiles.AddRange(DocumentFiles); Console.WriteLine($"[AddTopic] {DocumentFiles.Count} document files received."); }

                if (addedTopic != null && allFiles.Count > 0)
                {
                    Console.WriteLine($"[AddTopic] Processing {allFiles.Count} media files for topic {addedTopic.Id}.");
                    foreach (var file in allFiles)
                    {
                        if (file.Length > 0)
                        {
                            Console.WriteLine($"[AddTopic] Uploading file: {file.FileName} ({file.ContentType}), size: {file.Length} bytes");
                            using var stream = file.OpenReadStream();
                            var contentType = file.ContentType.ToLower();
                            CloudinaryDotNet.Actions.UploadResult uploadResult = null;

                            if (contentType.StartsWith("image"))
                            {
                                var uploadParams = new ImageUploadParams
                                {
                                    File = new FileDescription(file.FileName, stream),
                                    Folder = "topic_media"
                                };
                                uploadResult = cloudinary.Upload(uploadParams);
                            }
                            else if (contentType.StartsWith("video"))
                            {
                                var uploadParams = new VideoUploadParams
                                {
                                    File = new FileDescription(file.FileName, stream),
                                    Folder = "topic_media"
                                };
                                uploadResult = cloudinary.Upload(uploadParams);
                            }
                            else
                            {
                                var uploadParams = new RawUploadParams
                                {
                                    File = new FileDescription(file.FileName, stream),
                                    Folder = "topic_media"
                                };
                                uploadResult = cloudinary.Upload(uploadParams);
                            }

                            if (uploadResult.Error != null)
                            {
                                Console.WriteLine($"[AddTopic] ❌ Cloudinary upload failed: {uploadResult.Error.Message}");
                                throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                            }
                            Console.WriteLine($"[AddTopic] ✅ Upload succeeded. Secure URL: {uploadResult.SecureUrl}");

                            var topicMedia = new TopicMediaViewModel
                            {
                                TopicId = addedTopic.Id,
                                Name = file.FileName,
                                MediaType = file.ContentType,
                                MediaUrl = uploadResult.SecureUrl?.ToString(),
                                AccountId = model.AccountId
                            };
                            _topicMediaService.AddTopicMedia(topicMedia);
                            Console.WriteLine($"[AddTopic] Media saved for topic {addedTopic.Id}: {uploadResult.SecureUrl}");
                        }
                        else
                        {
                            Console.WriteLine($"[AddTopic] Skipping empty file: {file.FileName}");
                        }
                    }
                }
                else if (addedTopic != null)
                {
                    Console.WriteLine("[AddTopic] No media files to process.");
                }

                TempData["Success"] = "Topic and media added successfully!";
                Console.WriteLine("[AddTopic] Topic and media added successfully!");
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddTopic] Exception: {ex.Message}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
        }

        [HttpGet]
        public IActionResult TopicDetails(int topicId)
        {
            var topic = _topicService.GetTopicWithAccountById(topicId);
            var media = _topicMediaService.GetAllTopicMediaByTopicId(topicId);
            ViewBag.Topic = topic;
            ViewBag.TopicMedia = media;
            ViewBag.Account = topic.Account;
            return View("~/Views/Admin/AdminTopic.cshtml");
        }
    }
} 