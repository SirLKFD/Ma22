using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITopicRepository
    {
        IQueryable<Topic> GetTopics();
        bool TopicExists(string name);
        void AddTopic(Topic topic);
        void UpdateTopic(Topic topic);
        void DeleteTopic(Topic topic);
        Topic GetTopicWithAccountById(int id);
    }
}
