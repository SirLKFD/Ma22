using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories
{
    public class TrainingCategoryRepository : BaseRepository, ITrainingCategoryRepository
    {
        public TrainingCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<TrainingCategory> GetTrainingCategories()
        {
            return this.GetDbSet<TrainingCategory>().Include(c => c.Account);
        }

        public bool TrainingCategoryExists(string name)
        {
            return this.GetDbSet<TrainingCategory>().Any(x => x.CategoryName == name);
        }

         public void AddTrainingCategory(TrainingCategory trainingCategory)
        {
            Console.WriteLine($"Adding: {trainingCategory.CategoryName}, {trainingCategory.AccountId}, {trainingCategory.CreatedBy}, {trainingCategory.CreatedTime}");
            this.GetDbSet<TrainingCategory>().Add(trainingCategory);
            UnitOfWork.SaveChanges();
            Console.WriteLine("SaveChanges called.");
        }
    }
}
