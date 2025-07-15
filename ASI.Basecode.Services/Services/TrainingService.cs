using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.Services.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public TrainingService(ITrainingRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddTraining(TrainingViewModel model)
        {
            var training = new Training();
            if (!_repository.TrainingExists(model.TrainingName, model.TrainingCategoryId))
            {
                try
                {
                    Console.WriteLine("✅ Mapping Training");
                    _mapper.Map(model, training);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Exception during mapping: {ex.Message}");
                    throw;
                }
                training.CreatedTime = DateTime.Now;
                training.UpdatedTime = DateTime.Now;
                training.CreatedBy = System.Environment.UserName;
                training.UpdatedBy = System.Environment.UserName;

                Console.WriteLine($"[Service] Mapped Training: {training.TrainingName}, AccountId: {training.AccountId}, CreatedBy: {training.CreatedBy}, CreatedTime: {training.CreatedTime}");

                _repository.AddTraining(training);

                Console.WriteLine("[Service] AddTraining called on repository.");
            }
            else
            {
                Console.WriteLine($"[Service] ❌ Error: Training '{model.TrainingName}' already exists.");
                throw new InvalidDataException(Resources.Messages.Errors.TrainingExists);
            }
        }

        public List<TrainingViewModel> GetAllTrainingsByCategoryId(int categoryId)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");

            var query = _repository.GetTrainings()
                .Include(t => t.SkillLevelNavigation)
                .Include(t => t.TrainingCategory)
                .Where(t => t.TrainingCategoryId == categoryId);

            if (accountRole == 0) // Only superadmin sees all
                query = query.Where(t => t.AccountId == accountId);

            return query.Select(t => new TrainingViewModel
            {
                Id = t.Id,
                Ratings = t.Ratings,
                AccountId = t.AccountId,
                TrainingName = t.TrainingName,
                TrainingCategoryId = t.TrainingCategoryId,
                TrainingCategoryName = t.TrainingCategory.CategoryName,
                SkillLevel = t.SkillLevel,
                SkillLevelName = t.SkillLevelNavigation.SkillLevel1,
                Description = t.Description,
                CoverPicture = t.CoverPicture,
                Duration = t.Duration,
                CourseCode = t.CourseCode,
                UpdatedTime = t.UpdatedTime,
                AccountFirstName = t.Account.FirstName,
                AccountLastName = t.Account.LastName
            }).ToList();
        }

        public TrainingViewModel GetTrainingById(int id)
        {
            var training = _repository.GetTrainings().Include(t => t.DurationNavigation)
            .Include(t => t.SkillLevelNavigation).Include(t => t.TrainingCategory)
            .Include(t => t.Reviews)
            .ThenInclude(r => r.Account)
            .FirstOrDefault(t => t.Id == id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (training == null || (training.AccountId != accountId && accountRole == 0))
                throw new UnauthorizedAccessException("You are not allowed to view this training.");

            return new TrainingViewModel
            {
                Id = training.Id,
                Ratings = training.Ratings,
                AccountId = training.AccountId,
                TrainingName = training.TrainingName,
                TrainingCategoryName = training.TrainingCategory?.CategoryName,
                TrainingCategoryId = training.TrainingCategoryId,
                SkillLevel = training.SkillLevel,
                SkillLevelName = training.SkillLevelNavigation.SkillLevel1,
                Description = training.Description,
                CoverPicture = training.CoverPicture,
                Duration = training.Duration,
                DurationName = training.DurationNavigation?.Duration1,
                CourseCode = training.CourseCode,
                UpdatedTime = training.UpdatedTime,
                AccountPicture = training.Account.ProfilePicture,
                AccountFirstName = training.Account.FirstName,
                AccountLastName = training.Account.LastName,
                Reviews = training.Reviews.Select(t => new ReviewViewModel
                {
                    ReviewId = t.ReviewId,
                    TrainingId = t.TrainingId,
                    Title = t.Title,
                    UserReview = t.UserReview,
                    ReviewScore = t.ReviewScore,
                    AccountId = t.AccountId,
                    AccountFirstName = t.Account?.FirstName,
                    AccountLastName = t.Account?.LastName,
                    ProfilePicture = t.Account?.ProfilePicture,
                    CreatedTime = t.CreatedTime
                }).ToList()
            };
        }

        public List<TrainingViewModel> GetAllTrainings()
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            var query = _repository.GetTrainings()
                .Include(t => t.SkillLevelNavigation)
                .Include(t => t.TrainingCategory)
                .AsQueryable();
            if (accountRole == 0)
                query = query.Where(t => t.AccountId == accountId);
            return query.Select(t => new TrainingViewModel
            {
                Id = t.Id,
                Ratings = t.Ratings,
                AccountId = t.AccountId,
                TrainingName = t.TrainingName,
                TrainingCategoryId = t.TrainingCategoryId,
                TrainingCategoryName = t.TrainingCategory.CategoryName,
                SkillLevel = t.SkillLevel,
                SkillLevelName = t.SkillLevelNavigation.SkillLevel1,
                Description = t.Description,
                CoverPicture = t.CoverPicture,
                Duration = t.Duration,
                CourseCode = t.CourseCode,
                UpdatedTime = t.UpdatedTime,
                AccountFirstName = t.Account.FirstName,
                AccountLastName = t.Account.LastName
            }).ToList();
        }

        public void UpdateTraining(TrainingViewModel model)
        {
            var training = _repository.GetTrainings().FirstOrDefault(c => c.Id == model.Id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (training != null)
            {
                if (training.AccountId != accountId && accountRole == 0)
                    throw new UnauthorizedAccessException("You are not allowed to edit this training.");
                Console.WriteLine($"[Service] Attempting to edit training: {training.TrainingName}, AccountId: {training.AccountId}");
                if (!_repository.TrainingExists(training.TrainingName,training.TrainingCategoryId))
                {
                    Console.WriteLine($"[Service] ❌ Error: Training '{training.TrainingName}' already exists.");
                    throw new InvalidDataException(Resources.Messages.Errors.TrainingExists);
                }
                _mapper.Map(model, training);
                training.UpdatedTime = DateTime.Now;
                training.UpdatedBy = System.Environment.UserName;
                Console.WriteLine($"[Service] Mapped Training: {training.TrainingName}, AccountId: {training.AccountId}, UpdatedBy: {training.UpdatedBy}, UpdatedTime: {training.UpdatedTime}");
                _repository.UpdateTraining(training);
            }
        }

        public void DeleteTraining(int id)
        {
            var training = _repository.GetTrainings().FirstOrDefault(t => t.Id == id);
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            if (training != null)
            {
                if (training.AccountId != accountId && accountRole == 0)
                    throw new UnauthorizedAccessException("You are not allowed to delete this training.");
                _repository.DeleteTraining(training);
            }
        }

        public List<TrainingViewModel> GetPaginatedTrainings(string search, int page, int pageSize, out int totalCount)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            var query = _repository.GetTrainings();
            if (accountRole == 0)
                query = query.Where(t => t.AccountId == accountId);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TrainingName.Contains(search));
            }
            totalCount = query.Count();
            var paged = query
                .OrderByDescending(t => t.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TrainingViewModel
                {
                    Id = t.Id,
                    Ratings = t.Ratings,
                    AccountId = t.AccountId,
                    TrainingName = t.TrainingName,
                    TrainingCategoryId = t.TrainingCategoryId,
                    SkillLevel = t.SkillLevel,
                    Description = t.Description,
                    CoverPicture = t.CoverPicture,
                    Duration = t.Duration,
                    CourseCode = t.CourseCode,
                    UpdatedTime = t.UpdatedTime,
                    AccountFirstName = t.Account.FirstName,
                    AccountLastName = t.Account.LastName
                }).ToList();
            return paged;
        }

        public List<TrainingViewModel> GetFilteredTrainings(string search, int? categoryId, int? skillLevelId, int page, int pageSize, out int totalCount)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
            var query = _repository.GetTrainings()
                .Include(t => t.SkillLevelNavigation)
                .Include(t => t.TrainingCategory)
                .AsQueryable();

            if (accountRole == 0)
                query = query.Where(t => t.AccountId == accountId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TrainingName.Contains(search));
                Console.WriteLine($"[Service] Filtering by search: '{search}'");
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.TrainingCategoryId == categoryId.Value);
                Console.WriteLine($"[Service] Filtering by CategoryId: {categoryId.Value}");
            }

            if (skillLevelId.HasValue)
            {
                query = query.Where(t => t.SkillLevel == skillLevelId.Value);
                Console.WriteLine($"[Service] Filtering by SkillLevelId: {skillLevelId.Value}");
            }

            totalCount = query.Count();

            var paged = query.OrderByDescending(t => t.CreatedTime)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            Console.WriteLine($"[Service] Filtered and paginated trainings fetched: Page {page}, Count {paged.Count}, TotalCount {totalCount}");

            return paged.Select(training => new TrainingViewModel
            {
                Id = training.Id,
                Ratings = training.Ratings,
                AccountId = training.AccountId,
                TrainingName = training.TrainingName,
                TrainingCategoryId = training.TrainingCategoryId,
                TrainingCategoryName = training.TrainingCategory.CategoryName,
                SkillLevel = training.SkillLevel,
                SkillLevelName = training.SkillLevelNavigation.SkillLevel1,
                Description = training.Description,
                CoverPicture = training.CoverPicture,
                Duration = training.Duration,
                CourseCode = training.CourseCode,
                UpdatedTime = training.UpdatedTime,
                AccountFirstName = training.Account.FirstName,
                AccountLastName = training.Account.LastName,
            }).ToList();
        }

        public int GetFilteredTrainingsCount(string search, int? categoryId, int? skillLevelId)
        {
            var trainings = _repository.GetFilteredTrainings(search, categoryId, skillLevelId);
            return trainings.Count();
        }

    }
}