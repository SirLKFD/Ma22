using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITrainingCategoryRepository
    {
        IQueryable<TrainingCategory> GetTrainingCategories();
        TrainingCategory GetTrainingCategoryById(int id);
        void AddTrainingCategory(TrainingCategory category);
        void UpdateTrainingCategory(TrainingCategory category);
        bool TrainingCategoryExists(string categoryName);
        void DeleteTrainingCategory(TrainingCategory category);
        IQueryable<TrainingCategory> GetTrainingCategoriesByIds(IEnumerable<int> ids);
    }
}
