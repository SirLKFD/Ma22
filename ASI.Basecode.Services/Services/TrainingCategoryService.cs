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
    public class TrainingCategoryService : ITrainingCategoryService
    {
        private readonly ITrainingCategoryRepository _repository;
        private readonly IMapper _mapper;

        public TrainingCategoryService(ITrainingCategoryRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public void AddTrainingCategory(TrainingCategoryViewModel model)
        {
            Console.WriteLine($"[Service] Attempting to add category: {model.CategoryName}, AccountId: {model.AccountId}");

            var trainingCategory = new TrainingCategory();
            if (!_repository.TrainingCategoryExists(model.CategoryName))
            {
               try
                {
                    Console.WriteLine("✅ Mapping TrainingCategory");
                    _mapper.Map(model, trainingCategory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Exception during mapping: {ex.Message}");
                    throw;
                }
                trainingCategory.CreatedTime = DateTime.Now;
                trainingCategory.UpdatedTime = DateTime.Now;
                trainingCategory.CreatedBy = System.Environment.UserName;
                trainingCategory.UpdatedBy = System.Environment.UserName;

                Console.WriteLine($"[Service] Mapped TrainingCategory: {trainingCategory.CategoryName}, AccountId: {trainingCategory.AccountId}, CreatedBy: {trainingCategory.CreatedBy}, CreatedTime: {trainingCategory.CreatedTime}");

                _repository.AddTrainingCategory(trainingCategory);

                Console.WriteLine("[Service] AddTrainingCategory called on repository.");
            }
            else
            {
                Console.WriteLine($"[Service] ❌ Error: Category '{model.CategoryName}' already exists.");
                throw new InvalidDataException(Resources.Messages.Errors.TrainingCategoryExists);
            }
        }

        public void EditTrainingCategory(TrainingCategoryViewModel model)
        {
            var trainingCategory = _repository.GetTrainingCategories().FirstOrDefault(c => c.Id == model.Id);
            if (trainingCategory != null)
            {
                Console.WriteLine($"[Service] Attempting to edit category: {trainingCategory.CategoryName}, AccountId: {trainingCategory.AccountId}");
                if (!_repository.TrainingCategoryExists(trainingCategory.CategoryName))
                {
                    Console.WriteLine($"[Service] ❌ Error: Category '{trainingCategory.CategoryName}' already exists.");
                    throw new InvalidDataException(Resources.Messages.Errors.TrainingCategoryExists);
                }
                _mapper.Map(model, trainingCategory);
                trainingCategory.UpdatedTime = DateTime.Now;
                trainingCategory.UpdatedBy = System.Environment.UserName;
                Console.WriteLine($"[Service] Mapped TrainingCategory: {trainingCategory.CategoryName}, AccountId: {trainingCategory.AccountId}, UpdatedBy: {trainingCategory.UpdatedBy}, UpdatedTime: {trainingCategory.UpdatedTime}");
                _repository.UpdateTrainingCategory(trainingCategory);
            }
        }

        public void DeleteTrainingCategory(int id)
        {
            var trainingCategory = _repository.GetTrainingCategories().FirstOrDefault(c => c.Id == id);
            if (trainingCategory != null)
            {
                _repository.DeleteTrainingCategory(trainingCategory);
            }
        }
        public List<TrainingCategory> GetAllTrainingCategories()
        {
            return _repository.GetTrainingCategories().ToList();
        }

        public TrainingCategoryViewModel GetTrainingCategoryById(int id)
        {
            var category = _repository.GetTrainingCategories().FirstOrDefault(c => c.Id == id);
            return new TrainingCategoryViewModel
            {
                Id = category.Id,
                AccountId = category.AccountId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                CoverPicture = category.CoverPicture,
                UpdatedTime = category.UpdatedTime,
                AccountFirstName = category.Account.FirstName,
                AccountLastName = category.Account.LastName,
            };
        }

        public List<TrainingCategoryViewModel> GetAllTrainingCategoryViewModels()
        {
            var categories = _repository.GetTrainingCategories()
                .Select(c => new TrainingCategoryViewModel
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CoverPicture = c.CoverPicture,
                    UpdatedTime = c.UpdatedTime,
                    AccountFirstName = c.Account.FirstName,
                    AccountLastName = c.Account.LastName,
                }).ToList();
            return categories;
        }
    }
}
