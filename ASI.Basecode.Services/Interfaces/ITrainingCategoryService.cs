using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingCategoryService
    {
        void AddTrainingCategory(TrainingCategoryViewModel model);
        List<TrainingCategory> GetAllTrainingCategories();
        TrainingCategory GetTrainingCategoryById(int id);
    }
}
