using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class TopicMediaService : ITopicMediaService
    {
        private readonly ITopicMediaRepository _repository;
        private readonly IMapper _mapper;

        public TopicMediaService(ITopicMediaRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public void AddTopicMedia(TopicMediaViewModel model)
        {
            Console.WriteLine($"[Service] Attempting to add topic media: {model.TopicId}, AccountId: {model.AccountId}");

            var topicMedia = new TopicMedium();
      
            try{
                Console.WriteLine("✅ Mapping Topic");
                _mapper.Map(model, topicMedia);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during mapping: {ex.Message}");
                throw;
            }
             
            topicMedia.CreatedTime = DateTime.Now;
            topicMedia.UpdatedTime = DateTime.Now;
            topicMedia.CreatedBy = System.Environment.UserName;
            topicMedia.UpdatedBy = System.Environment.UserName;

            Console.WriteLine($"[Service] Mapped Topic: {topicMedia.TopicId}, AccountId: {topicMedia.AccountId}, CreatedBy: {topicMedia.CreatedBy}, CreatedTime: {topicMedia.CreatedTime}");

            _repository.AddTopicMedia(topicMedia);

            Console.WriteLine("[Service] AddTopicMedia called on repository.");
        }

        public List<TopicMedium> GetAllTopicMediaByTopicId(int topicId) 
        {
            return _repository.GetTopicMedia().Where(t => t.TopicId == topicId).ToList();
        }
    }
}
