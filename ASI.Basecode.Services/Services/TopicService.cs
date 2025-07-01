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

        public List<TopicViewModel> GetAllTopicsByTrainingId(int trainingId)
        {
            var topics = _repository.GetTopics().Where(t => t.TrainingId == trainingId)
                .OrderByDescending(t => t.CreatedTime)
                .Select(t => new TopicViewModel
                {
                    Id = t.Id,
                    TopicName = t.TopicName,
                    TrainingId = t.TrainingId,
                    Description = t.Description,
                    UpdatedTime = t.UpdatedTime,
                    Media = t.TopicMedia.Select(m => new TopicMediaViewModel {
                        Id = m.Id,
                        TopicId = m.TopicId,
                        MediaType = m.MediaType,
                        Name = m.Name,
                        MediaUrl = m.MediaUrl,
                        AccountId = m.AccountId
                    }).ToList(),
                    AccountFirstName = t.Account.FirstName,
                    AccountLastName = t.Account.LastName,
                    MediaCount = t.TopicMedia.Count
                }).ToList();

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

        public void UpdateTopic(TopicViewModel model)
        {
            var topic = _repository.GetTopicWithAccountById(model.Id);
            if (topic != null)
            {
                if (_repository.TopicExists(model.TopicName) && model.TopicName != topic.TopicName)
                {
                    Console.WriteLine($"[TopicService] ❌ Error: Topic '{model.TopicName}' already exists.");
                    throw new InvalidDataException(Resources.Messages.Errors.TopicExists);
                }
                _mapper.Map(model, topic);
                topic.UpdatedTime = DateTime.Now;
                topic.UpdatedBy = System.Environment.UserName;
                _repository.UpdateTopic(topic);
            }
        }

        public void DeleteTopic(int id)
        {
            var topic = _repository.GetTopicWithAccountById(id);
            if (topic != null)
            {
                _repository.DeleteTopic(topic);
            }
        }
    }
}
