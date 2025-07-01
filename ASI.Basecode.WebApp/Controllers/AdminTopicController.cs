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
using System.Text.Json;

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
            List<IFormFile> VideoFilesAdd,
            List<IFormFile> ImageFilesAdd,
            List<IFormFile> DocumentFilesAdd,
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
                    .OrderByDescending(t => t.UpdatedTime).FirstOrDefault(t => t.TopicName == model.TopicName);
                if (addedTopic != null)
                {
                    Console.WriteLine($"[AddTopic] Fetched added topic with ID: {addedTopic.Id}");
                }
                else
                {
                    Console.WriteLine("[AddTopic] Could not fetch the newly added topic.");
                }

                var allFiles = new List<IFormFile>();
                if (VideoFilesAdd != null) { allFiles.AddRange(VideoFilesAdd); Console.WriteLine($"[AddTopic] {VideoFilesAdd.Count} video files received."); }
                if (ImageFilesAdd != null) { allFiles.AddRange(ImageFilesAdd); Console.WriteLine($"[AddTopic] {ImageFilesAdd.Count} image files received."); }
                if (DocumentFilesAdd != null) { allFiles.AddRange(DocumentFilesAdd); Console.WriteLine($"[AddTopic] {DocumentFilesAdd.Count} document files received."); }

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
            Console.WriteLine("topicId is",topicId);
            var topic = _topicService.GetTopicWithAccountById(topicId);
            var topics = _topicService.GetAllTopicsByTrainingId(topic.TrainingId);

            ViewData["topics"] = topics;
   
            return View("~/Views/Admin/AdminTopic.cshtml", topic);
        }

          [HttpPost]
        public IActionResult EditTopic(
            TopicViewModel model,
            List<IFormFile> VideoFilesEdit,
            List<IFormFile> ImageFilesEdit,
            List<IFormFile> DocumentFilesEdit,
            string DeletedMediaIds,
            [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {

            Console.WriteLine($"[EditTopic] ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"[EditTopic] Incoming model.Id: {model.Id}, model.AccountId: {model.AccountId}, model.TopicName: {model.TopicName}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[EditTopic] ModelState is invalid. Redirecting.");
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }

            try
            {
                Console.WriteLine($"[EditTopic] Model: {JsonSerializer.Serialize(model)}");
                var existingTopic = _topicService.GetTopicById(model.Id);
                Console.WriteLine($"[EditTopic] existingTopic.Id: {existingTopic?.Id}, existingTopic.AccountId: {existingTopic?.AccountId}");
                model.AccountId = existingTopic.AccountId;
                model.AccountFirstName = existingTopic.Account.FirstName;
                model.AccountLastName = existingTopic.Account.LastName;
                Console.WriteLine($"[EditTopic] After assignment, model.AccountId: {model.AccountId}");
                _topicService.UpdateTopic(model);
                Console.WriteLine("[EditTopic] Called _topicService.UpdateTopic");

                var updatedTopic = _topicService.GetAllTopicsByTrainingId(model.TrainingId)
                    .OrderByDescending(t => t.UpdatedTime).FirstOrDefault(t => t.TopicName == model.TopicName);
                Console.WriteLine($"[EditTopic] updatedTopic.Id: {updatedTopic?.Id}, updatedTopic.AccountId: {updatedTopic?.AccountId}");

                var allFiles = new List<IFormFile>();
                if (VideoFilesEdit != null) { allFiles.AddRange(VideoFilesEdit); Console.WriteLine($"[EditTopic] {VideoFilesEdit.Count} video files received."); }
                if (ImageFilesEdit != null) { allFiles.AddRange(ImageFilesEdit); Console.WriteLine($"[EditTopic] {ImageFilesEdit.Count} image files received."); }
                if (DocumentFilesEdit != null) { allFiles.AddRange(DocumentFilesEdit); Console.WriteLine($"[EditTopic] {DocumentFilesEdit.Count} document files received."); }

                if (updatedTopic != null && allFiles.Count > 0)
                {
                    Console.WriteLine($"[EditTopic] Processing {allFiles.Count} media files for topic {updatedTopic.Id}.");
                    foreach (var file in allFiles)
                    {
                        if (file.Length > 0)
                        {
                            Console.WriteLine($"[EditTopic] Uploading file: {file.FileName} ({file.ContentType}), size: {file.Length} bytes");
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
                                Console.WriteLine($"[EditTopic] ❌ Cloudinary upload failed: {uploadResult.Error.Message}");
                                throw new Exception("Cloudinary upload failed: " + uploadResult.Error.Message);
                            }
                            Console.WriteLine($"[EditTopic] ✅ Upload succeeded. Secure URL: {uploadResult.SecureUrl}");

                            var topicMedia = new TopicMediaViewModel
                            {
                                TopicId = updatedTopic.Id,
                                Name = file.FileName,
                                MediaType = file.ContentType,
                                MediaUrl = uploadResult.SecureUrl?.ToString(),
                                AccountId = model.AccountId
                            };
                            Console.WriteLine($"[EditTopic] About to add TopicMedia: TopicId={topicMedia.TopicId}, Name={topicMedia.Name}, MediaType={topicMedia.MediaType}, MediaUrl={topicMedia.MediaUrl}, AccountId={topicMedia.AccountId}");
                            _topicMediaService.AddTopicMedia(topicMedia);
                            Console.WriteLine($"[EditTopic] Media saved for topic {updatedTopic.Id}: {uploadResult.SecureUrl}");
                        }
                        else
                        {
                            Console.WriteLine($"[EditTopic] Skipping empty file: {file.FileName}");
                        }
                    }
                }
                else if (updatedTopic != null)
                {
                    Console.WriteLine("[EditTopic] No media files to process.");
                }

                // Handle deletions
                if (!string.IsNullOrEmpty(DeletedMediaIds))
                {
                    var idsToDelete = DeletedMediaIds.Split(',').Select(int.Parse).ToList();
                    foreach (var mediaId in idsToDelete)
                    {
                        var media = _topicMediaService.GetTopicMediaById(mediaId);
                        Console.WriteLine($"[EditTopic] Deleting mediaId: {mediaId}, found: {media != null}");
                        if (media != null)
                        {
                            _topicMediaService.DeleteTopicMedia(mediaId);
                            Console.WriteLine($"[EditTopic] Deleted mediaId: {mediaId}");
                        }
                    }
                }

                TempData["Success"] = "Topic and media updated successfully!";
                Console.WriteLine("[AddTopic] Topic and media updated successfully!");
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EditTopic] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
        }

        [HttpPost]
        public IActionResult DeleteTopic(int id)
        {
            var topic = _topicService.GetTopicWithAccountById(id);
            if (topic != null)
            {
                _topicService.DeleteTopic(id);
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = topic.TrainingId });
            }
            return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = topic.TrainingId });
        }
    }   
} 