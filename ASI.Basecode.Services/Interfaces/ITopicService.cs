using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITopicService
    {
        void AddTopic(TopicViewModel model);
        List<Topic> GetAllTopicsByTrainingId(int trainingId);
        Topic GetTopicById(int id);
        Topic GetTopicWithAccountById(int id);
    }
}
