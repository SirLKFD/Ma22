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

        public List<TrainingCategory> GetAllTrainingCategories()
        {
            return _repository.GetTrainingCategories().ToList();
        }

        public TrainingCategory GetTrainingCategoryById(int id)
        {
            return _repository.GetTrainingCategories().FirstOrDefault(c => c.Id == id);
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
