using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories
{
    public class TopicRepository : BaseRepository, ITopicRepository
    {
        public TopicRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Topic> GetTopics()
        {
            return this.GetDbSet<Topic>().Include(t => t.Account);
        }

        public Topic GetTopicWithAccountById(int id)
        {
            return this.GetDbSet<Topic>()
                .Include(t => t.Account)
                .Include(t => t.TopicMedia)
                .FirstOrDefault(t => t.Id == id);
        }

        public bool TopicExists(string name, int trainingId)
        {
            return this.GetDbSet<Topic>().Any(x => x.TopicName == name && x.TrainingId == trainingId);
        }

         public void AddTopic(Topic topic)
        {
            Console.WriteLine($"Adding: {topic.TopicName}, {topic.AccountId}, {topic.CreatedBy}, {topic.CreatedTime}");
            this.GetDbSet<Topic>().Add(topic);
            UnitOfWork.SaveChanges();
            Console.WriteLine("SaveChanges called.");
        }

        public void UpdateTopic(Topic topic)
        {
            var existingTopic = this.GetDbSet<Topic>().FirstOrDefault(x => x.Id == topic.Id);
            if (existingTopic != null)
            {
                existingTopic.TopicName = topic.TopicName;
                existingTopic.Description = topic.Description;
                existingTopic.UpdatedTime = topic.UpdatedTime;
                existingTopic.UpdatedBy = topic.UpdatedBy;
                UnitOfWork.SaveChanges();
                Console.WriteLine("SaveChanges called.");
            }
        }

        public void DeleteTopic(Topic topic)
        {
            var existingTopic = this.GetDbSet<Topic>().FirstOrDefault(x => x.Id == topic.Id);
            if (existingTopic != null)
            {
                this.GetDbSet<Topic>().Remove(existingTopic);
                UnitOfWork.SaveChanges();
                Console.WriteLine("SaveChanges called.");
            }
        }
    }
}
