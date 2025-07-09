using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("admin/[action]")]
    [Authorize(Roles = "0,2")]
    public class AdminTopicController : ControllerBase<AdminTopicController>
    {
        private readonly ITopicService _topicService;
        private readonly ITopicMediaService _topicMediaService;
        private readonly IUserService _userService;

        private readonly IAuditLogService _auditLogService;

        public AdminTopicController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper = null,
            ITopicService topicService = null,
            ITopicMediaService topicMediaService = null,
            IAuditLogService auditLogService= null,
            IUserService userService = null
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _topicService = topicService;
            _auditLogService = auditLogService;
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

                 _auditLogService.LogAction("Topic", "Create", addedTopic.Id, accountId.Value,addedTopic.TopicName);

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

                using var memoryStream = new MemoryStream();
                using (var zip = new System.IO.Compression.ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var media in mediaList)
                    {
                        if (string.IsNullOrWhiteSpace(media.MediaUrl))
                            continue;

                        try
                        {
                            using var client = new HttpClient();
                            var fileData = await client.GetByteArrayAsync(media.MediaUrl);
                            var entry = zip.CreateEntry(media.Name ?? $"media_{media.Id}");

                            using var entryStream = entry.Open();
                            await entryStream.WriteAsync(fileData, 0, fileData.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[DownloadMediaZip] Skipped media {media.Id}: {ex.Message}");
                        }
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                // Sanitize topicTitle for file name
                string safeTitle = string.IsNullOrWhiteSpace(topicTitle) ? $"Topic_{topicId}" : string.Join("_", topicTitle.Split(Path.GetInvalidFileNameChars()));
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
                    // Determine which media type is selected
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
                        // Return the single file directly
                        var media = mediaList.First();
                        using var client = new HttpClient();
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
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadMediaZip] Exception: {ex.Message}");
                TempData["Error"] = "Failed to download ZIP: " + ex.Message;
                return RedirectToAction("TopicDetails", new { topicId });
            }
        }

        // Add these helper methods to the controller
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
            // Check by MIME type first
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

            // Check by file extension as fallback
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

        [HttpGet]
        public IActionResult TopicDetails(int topicId)
        {
            var topic = _topicService.GetTopicWithAccountById(topicId);
            var allTopics = _topicService.GetAllTopicsByTrainingId(topic.TrainingId);
            ViewBag.AllTopics = allTopics;
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }

            try
            {
                Console.WriteLine(model);
                int? accountId = HttpContext.Session.GetInt32("AccountId");
                var existingTopic = _topicService.GetTopicWithAccountById(model.Id);
                model.AccountId = existingTopic.AccountId;
                model.AccountFirstName = existingTopic.AccountFirstName;
                model.AccountLastName = existingTopic.AccountLastName;

                _topicService.UpdateTopic(model);


                var updatedTopic = _topicService.GetAllTopicsByTrainingId(model.TrainingId)
                    .OrderByDescending(t => t.UpdatedTime).FirstOrDefault(t => t.TopicName == model.TopicName);

                 _auditLogService.LogAction("Topic", "Update", updatedTopic.Id, accountId.Value,updatedTopic.TopicName);

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
                        if (media != null)
                        {
                            _topicMediaService.DeleteTopicMedia(mediaId);
                        }
                    }
                }

                TempData["Success"] = "Topic and media updated successfully!";
                Console.WriteLine("[EditTopic] Topic and media updated successfully!");
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EditTopic] Exception: {ex.Message}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = model.TrainingId });
            }
        }

        [HttpPost]
        public IActionResult DeleteTopic(int id)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountId");
            var topic = _topicService.GetTopicWithAccountById(id);
            if (topic != null)
            {
                _auditLogService.LogAction("Topic", "Delete", id, accountId.Value, topic.TopicName);
                _topicService.DeleteTopic(id);
                
                return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = topic.TrainingId });
            }
            return RedirectToAction("AdminTrainingTopics", "AdminTraining", new { trainingId = topic.TrainingId });
        }

        [HttpGet]
        public IActionResult MediaModalPartial(int topicId, string type, int index)
        {
            var topic = _topicService.GetTopicWithAccountById(topicId);
            var mediaList = topic.Media;
            ViewBag.CurrentIndex = index;
            ViewBag.Type = type;
            return PartialView("~/Views/Admin/_MediaModalPartial.cshtml", mediaList);
        }
    }
}