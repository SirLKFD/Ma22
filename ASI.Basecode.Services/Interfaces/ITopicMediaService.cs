using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITopicMediaService
    {
        void AddTopicMedia(TopicMediaViewModel model);
        List<TopicMediaViewModel> GetAllTopicMediaByTopicId(int topicId);
        void DeleteTopicMedia(int mediaId);
        TopicMediaViewModel GetTopicMediaById(int mediaId);
    }
}
