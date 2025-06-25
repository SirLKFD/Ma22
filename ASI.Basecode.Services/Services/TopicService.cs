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
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _repository;
        private readonly IMapper _mapper;

        public TopicService(ITopicRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public void AddTopic(TopicViewModel model)
        {
            Console.WriteLine($"[TopicService] Attempting to add topic: {model.TopicName}, AccountId: {model.AccountId}");

            var topic = new Topic();
            if (!_repository.TopicExists(model.TopicName))
            {
               try
                {
                    Console.WriteLine("[TopicService] Mapping TopicViewModel to Topic entity.");
                    _mapper.Map(model, topic);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TopicService] ❌ Exception during mapping: {ex.Message}");
                    throw;
                }
                topic.CreatedTime = DateTime.Now;
                topic.UpdatedTime = DateTime.Now;
                topic.CreatedBy = System.Environment.UserName;
                topic.UpdatedBy = System.Environment.UserName;

                Console.WriteLine($"[TopicService] Mapped Topic: {topic.TopicName}, AccountId: {topic.AccountId}, CreatedBy: {topic.CreatedBy}, CreatedTime: {topic.CreatedTime}");

                _repository.AddTopic(topic);

                Console.WriteLine("[TopicService] AddTopic called on repository.");
            }
            else
            {
                Console.WriteLine($"[TopicService] ❌ Error: Topic '{model.TopicName}' already exists.");
                throw new InvalidDataException(Resources.Messages.Errors.TopicExists);
            }
        }

        public List<Topic> GetAllTopicsByTrainingId(int trainingId)
        {
            Console.WriteLine($"[TopicService] Fetching all topics for TrainingId: {trainingId}");
            var topics = _repository.GetTopics().Where(t => t.TrainingId == trainingId).ToList();
            Console.WriteLine($"[TopicService] Found {topics.Count} topics for TrainingId: {trainingId}");
            return topics;
        }

        public Topic GetTopicById(int id)
        {
            Console.WriteLine($"[TopicService] Fetching topic by Id: {id}");
            var topic = _repository.GetTopics().FirstOrDefault(t => t.Id == id);
            if (topic != null)
                Console.WriteLine($"[TopicService] Found topic: {topic.TopicName}");
            else
                Console.WriteLine($"[TopicService] No topic found for Id: {id}");
            return topic;
        }

        public Topic GetTopicWithAccountById(int id)
        {
            Console.WriteLine($"[TopicService] Fetching topic with account by Id: {id}");
            var topic = _repository.GetTopicWithAccountById(id);
            Console.WriteLine($"[TopicService] Found topic: {topic.TopicName}");
            return topic;
        }
    }
}
