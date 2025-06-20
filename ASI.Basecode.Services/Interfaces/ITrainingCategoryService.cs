using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITrainingCategoryService
    {
        void AddTrainingCategory(TrainingCategoryViewModel model);
    }
}
