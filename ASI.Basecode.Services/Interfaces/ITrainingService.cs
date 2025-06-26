using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingService
    {
        void AddTraining(TrainingViewModel model);
        List<Training> GetAllTrainings();
        List<Training> GetAllTrainingsByCategoryId(int categoryId);
        Training GetTrainingById(int id);
        
    }
}
