using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingService
    {
        void AddTraining(TrainingViewModel model);
        List<TrainingViewModel> GetAllTrainings();
        List<TrainingViewModel> GetAllTrainingsByCategoryId(int categoryId);
        TrainingViewModel GetTrainingById(int id);
        void UpdateTraining(TrainingViewModel model);
        void DeleteTraining(int id);
        // Pagination for trainings
        List<TrainingViewModel> GetPaginatedTrainings(string search, int page, int pageSize, out int totalCount);
        List<TrainingViewModel> GetFilteredTrainings(string search, int? categoryId, int? skillLevelId, int page, int pageSize, out int totalCount);
    }
}
