using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingCategoryService
    {
        void AddTrainingCategory(TrainingCategoryViewModel model);

        void EditTrainingCategory(TrainingCategoryViewModel model);

        void DeleteTrainingCategory(int id);
        List<ASI.Basecode.Data.Models.TrainingCategory> GetAllTrainingCategories();
        TrainingCategoryViewModel GetTrainingCategoryById(int id);
        List<TrainingCategoryViewModel> GetAllTrainingCategoryViewModels();
        List<TrainingCategoryViewModel> GetPaginatedTrainingCategories(string search, int page, int pageSize, out int totalCount);
        List<TrainingCategoryViewModel> GetTrainingCategoryViewModelsByIds(IEnumerable<int> ids);
        List<TrainingCategoryViewModel> GetAllTrainingCategoryViewModelsUnfiltered();
        int GetFilteredTrainingCategoriesCount(string search);
    }
}
