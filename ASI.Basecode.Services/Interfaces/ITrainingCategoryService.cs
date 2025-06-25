using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingCategoryService
    {
        void AddTrainingCategory(TrainingCategoryViewModel model);
        List<ASI.Basecode.Data.Models.TrainingCategory> GetAllTrainingCategories();
        ASI.Basecode.Data.Models.TrainingCategory GetTrainingCategoryById(int id);
        List<TrainingCategoryViewModel> GetAllTrainingCategoryViewModels();
    }
}
