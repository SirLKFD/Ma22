using System;
using System.Linq;

namespace ASI.Basecode.Services.Helpers
{
    public static class MediaHelper
    {
        public static string GetFileExtension(string fileName)
        {
            return fileName != null && fileName.Contains(".") ? fileName.Split('.').Last().ToUpper() : "";
        }

        public static string GetFileExtensionColor(string extension)
        {
            switch (extension.ToLower())
            {
                case "pdf": return "bg-red-500";
                case "doc":
                case "docx": return "bg-blue-500";
                case "xls":
                case "xlsx": return "bg-green-500";
                case "ppt":
                case "pptx": return "bg-orange-500";
                default: return "bg-gray-500";
            }
        }

        public static bool IsPdfFile(string mediaType, string fileName)
        {
            return (mediaType != null && mediaType.ToLower().Contains("pdf")) ||
                   (fileName != null && fileName.ToLower().EndsWith(".pdf"));
        }

        public static bool IsDocumentFile(string mediaType, string fileName)
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
    }
}
