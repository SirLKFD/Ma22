using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{   
    public class TopicMediaRepository : BaseRepository, ITopicMediaRepository
    {
        public TopicMediaRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<TopicMedium> GetTopicMedia()
        {
            return this.GetDbSet<TopicMedium>();
        }


         public void AddTopicMedia(TopicMedium topicMedia)
        {
            Console.WriteLine($"Adding: {topicMedia.TopicId}, {topicMedia.AccountId}, {topicMedia.CreatedBy}, {topicMedia.CreatedTime}");
            this.GetDbSet<TopicMedium>().Add(topicMedia);
            UnitOfWork.SaveChanges();
            Console.WriteLine("SaveChanges called.");
        }
    }
}
