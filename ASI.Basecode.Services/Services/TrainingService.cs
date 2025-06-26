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

        public List<Training> GetAllTrainingsByCategoryId(int categoryId)
        {
            return _repository.GetTrainings().Where(t => t.TrainingCategoryId == categoryId).ToList();
        }

        public Training GetTrainingById(int id)
        {
            return _repository.GetTrainings().FirstOrDefault(t => t.Id == id);
        }

        public List<Training> GetAllTrainings()
        {
            return _repository.GetTrainings().ToList();
        }
    }
}
