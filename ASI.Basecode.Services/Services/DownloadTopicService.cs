using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public interface IDownloadTopicService
    {
        Task<byte[]> GenerateDownloadZipAsync(int topicId, string mode, List<int>? selectedMediaIds = null);
    }

    public class DownloadTopicService : IDownloadTopicService
    {
        private readonly ITopicService _topicService;

        public DownloadTopicService(ITopicService topicService)
        {
            _topicService = topicService;
        }

        public async Task<byte[]> GenerateDownloadZipAsync(int topicId, string mode, List<int>? selectedMediaIds = null)
        {
            var topic = _topicService.GetTopicWithAccountById(topicId);
            if (topic == null || topic.Media == null || !topic.Media.Any())
                return Array.Empty<byte>();

            var filesToInclude = new List<TopicMediaViewModel>();

            switch (mode.ToLower())
            {
                case "all":
                    filesToInclude = topic.Media;
                    break;
                case "videos":
                    filesToInclude = topic.Media.Where(m => m.MediaType != null && m.MediaType.ToLower().Contains("video")).ToList();
                    break;
                case "documents":
                    filesToInclude = topic.Media.Where(m => IsDocumentFile(m.MediaType, m.Name)).ToList();
                    break;
                case "images":
                    filesToInclude = topic.Media.Where(m => m.MediaType != null && m.MediaType.ToLower().Contains("image")).ToList();
                    break;
                case "selected":
                    if (selectedMediaIds != null)
                        filesToInclude = topic.Media.Where(m => selectedMediaIds.Contains(m.Id)).ToList();
                    break;
                default:
                    break;
            }

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var media in filesToInclude)
                {
                    if (!string.IsNullOrEmpty(media.MediaUrl))
                    {
                        // Download the file content from URL (you might want to inject HttpClient or file storage service here)
                        var fileContent = await DownloadFileAsync(media.MediaUrl);
                        var fileName = Path.GetFileName(media.MediaUrl);

                        var entry = archive.CreateEntry(fileName);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(fileContent, 0, fileContent.Length);
                    }
                }
            }
            return memoryStream.ToArray();
        }

        private async Task<byte[]> DownloadFileAsync(string url)
        {
            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(url);
        }

        private bool IsDocumentFile(string mediaType, string fileName)
        {
            var lowerMediaType = mediaType?.ToLower();
            var lowerFileName = fileName?.ToLower();

            return (lowerMediaType != null && (lowerMediaType.Contains("pdf") ||
                                               lowerMediaType.Contains("word") ||
                                               lowerMediaType.Contains("excel") ||
                                               lowerMediaType.Contains("powerpoint") ||
                                               lowerMediaType.Contains("spreadsheet") ||
                                               lowerMediaType.Contains("presentation") ||
                                               lowerMediaType.Contains("msword") ||
                                               lowerMediaType.Contains("vnd.openxmlformats") ||
                                               lowerMediaType.Contains("vnd.ms-"))) ||
                   (lowerFileName != null && (lowerFileName.EndsWith(".pdf") ||
                                              lowerFileName.EndsWith(".doc") ||
                                              lowerFileName.EndsWith(".docx") ||
                                              lowerFileName.EndsWith(".xls") ||
                                              lowerFileName.EndsWith(".xlsx") ||
                                              lowerFileName.EndsWith(".ppt") ||
                                              lowerFileName.EndsWith(".pptx")));
        }
    }
}
