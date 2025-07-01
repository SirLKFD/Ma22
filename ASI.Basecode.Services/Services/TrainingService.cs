using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _repository;
        private readonly IMapper _mapper;

        public TrainingService(ITrainingRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public void AddTraining(TrainingViewModel model)
        {

            var training = new Training();
            if (!_repository.TrainingExists(model.TrainingName))
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
            return _repository.GetTrainings().Where(t => t.TrainingCategoryId == categoryId).Select(t => new TrainingViewModel
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
        }

        public TrainingViewModel GetTrainingById(int id)
        {
            var training = _repository.GetTrainings().FirstOrDefault(t => t.Id == id);
            if (training == null) return null;
            
            return new TrainingViewModel
            {
                Id = training.Id,
                Ratings = training.Ratings,
                AccountId = training.AccountId,
                TrainingName = training.TrainingName,
                TrainingCategoryId = training.TrainingCategoryId,
                SkillLevel = training.SkillLevel,
                Description = training.Description,
                CoverPicture = training.CoverPicture,
                Duration = training.Duration,
                CourseCode = training.CourseCode,
                UpdatedTime = training.UpdatedTime,
                AccountFirstName = training.Account.FirstName,
                AccountLastName = training.Account.LastName,
            };
        }

        public List<TrainingViewModel> GetAllTrainings()
        {
            return _repository.GetTrainings().Select(t => new TrainingViewModel
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
        }

        public void UpdateTraining(TrainingViewModel model)
        {
            var training = _repository.GetTrainings().FirstOrDefault(c => c.Id == model.Id);
            if (training != null)
            {
                Console.WriteLine($"[Service] Attempting to edit training: {training.TrainingName}, AccountId: {training.AccountId}");
                if (!_repository.TrainingExists(training.TrainingName))
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
            if (training != null)
            {
                _repository.DeleteTraining(training);
            }
        }

        public List<TrainingViewModel> GetPaginatedTrainings(string search, int page, int pageSize, out int totalCount)
        {
            var query = _repository.GetTrainings();
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
    }
}
