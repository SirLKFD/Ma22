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
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.Services.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
     
        public TopicService(ITopicRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddTopic(TopicViewModel model)
        {
            Console.WriteLine($"[TopicService] Attempting to add topic: {model.TopicName}, AccountId: {model.AccountId}");

            var topic = new Topic();
            if (!_repository.TopicExists(model.TopicName, model.TrainingId))
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
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            var query = _repository.GetTopics().Where(t => t.TrainingId == trainingId);
            if (accountRole == 0)
                query = query.Where(t => t.AccountId == accountId);
            var topics = query.OrderByDescending(t => t.CreatedTime)
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
                        AccountId = m.AccountId,
                        FileSize = m.FileSize
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
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (topic == null || (topic.AccountId != accountId && accountRole == 0))
                throw new UnauthorizedAccessException("You are not allowed to view this topic.");
            if (topic != null)
                Console.WriteLine($"[TopicService] Found topic: {topic.TopicName}");
            else
                Console.WriteLine($"[TopicService] No topic found for Id: {id}");
            return topic;
        }

        public TopicViewModel GetTopicWithAccountById(int id)
        {
            Console.WriteLine($"[TopicService] Fetching topic with account by Id: {id}");
            var topic = _repository.GetTopicWithAccountById(id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (topic == null || (topic.AccountId != accountId && accountRole == 0))
                throw new UnauthorizedAccessException("You are not allowed to view this topic.");
            if (topic != null)
                Console.WriteLine($"[TopicService] Found topic: {topic.TopicName}");
            return new TopicViewModel
            {
                Id = topic.Id,
                TopicName = topic.TopicName,
                TrainingId = topic.TrainingId,
                Description = topic.Description,
                UpdatedTime = topic.UpdatedTime,
                AccountId = topic.AccountId,
                AccountFirstName = topic.Account?.FirstName,
                AccountLastName = topic.Account?.LastName,
                Media = topic.TopicMedia?.Select(m => new TopicMediaViewModel {
                    Id = m.Id,
                    TopicId = m.TopicId,
                    MediaType = m.MediaType,
                    Name = m.Name,
                    MediaUrl = m.MediaUrl,
                    AccountId = m.AccountId,
                    FileSize = m.FileSize
                }).ToList() ?? new List<TopicMediaViewModel>(),
                MediaCount = topic.TopicMedia?.Count ?? 0
            };
        }

        public void UpdateTopic(TopicViewModel model)
        {
            var topic = _repository.GetTopicWithAccountById(model.Id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (topic != null)
            {
                if (topic.AccountId != accountId && accountRole == 0)
                    throw new UnauthorizedAccessException("You are not allowed to edit this topic.");
                if (_repository.TopicExists(model.TopicName, model.TrainingId) && model.TopicName != topic.TopicName)
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
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (topic != null)
            {
                if (topic.AccountId != accountId && accountRole == 0)
                    throw new UnauthorizedAccessException("You are not allowed to delete this topic.");
                _repository.DeleteTopic(topic);
            }
        }
    }
}
