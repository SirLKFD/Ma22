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
    public class TopicMediaService : ITopicMediaService
    {
        private readonly ITopicMediaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TopicMediaService(ITopicMediaRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
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

        public List<TopicMediaViewModel> GetAllTopicMediaByTopicId(int topicId) 
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            var query = _repository.GetTopicMedia().Where(t => t.TopicId == topicId);
            if (accountRole == 0)
                query = query.Where(t => t.AccountId == accountId);
            var media = query.OrderByDescending(t => t.CreatedTime)
                .Select(t => new TopicMediaViewModel
                {
                    Id = t.Id,
                    TopicId = t.TopicId,
                    MediaType = t.MediaType,
                    Name = t.Name,
                    MediaUrl = t.MediaUrl,
                    AccountId = t.AccountId
                }).ToList();

            return media;
        }

        public TopicMediaViewModel GetTopicMediaById(int id)
        {
            var media = _repository.GetTopicMedia().FirstOrDefault(m => m.Id == id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (media == null || (media.AccountId != accountId && accountRole == 0))
                throw new UnauthorizedAccessException("You are not allowed to view this media.");
            return new TopicMediaViewModel
                {
                    TopicId = media.TopicId,
                    MediaType = media.MediaType,
                    Name = media.Name,
                    MediaUrl = media.MediaUrl,
                    AccountId = media.AccountId,
                    Id = media.Id,
                    FileSize = media.FileSize
                };
        }

        public void DeleteTopicMedia(int id)
        {
            var media = _repository.GetTopicMedia().FirstOrDefault(m => m.Id == id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (media != null)
            {
                if (media.AccountId != accountId && accountRole == 0)
                    throw new UnauthorizedAccessException("You are not allowed to delete this media.");
                _repository.DeleteTopicMedia(id);
            }
        }
    }
}
