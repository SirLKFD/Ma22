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
    public class TrainingRepository : BaseRepository, ITrainingRepository
    {
        public TrainingRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Training> GetTrainings()
        {
            return this.GetDbSet<Training>().Include(t => t.Account).Include(t => t.TrainingCategory);
        }

        public bool TrainingNameExists(string name, int categoryId)
        {
            return this.GetDbSet<Training>().Any(x => x.TrainingName == name && x.TrainingCategoryId == categoryId);
        }

        public bool TrainingExists(string name, int categoryId)
        {
            return TrainingNameExists(name, categoryId);
        }

         public void AddTraining(Training training)
        {
            this.GetDbSet<Training>().Add(training);
            UnitOfWork.SaveChanges();
        }

        public void UpdateTraining(Training training)
        {
            var existingTraining = this.GetDbSet<Training>().FirstOrDefault(x => x.Id == training.Id);
            if (existingTraining != null)
            {
                existingTraining.TrainingName = training.TrainingName;
                existingTraining.TrainingCategoryId = training.TrainingCategoryId;
                existingTraining.SkillLevel = training.SkillLevel;
                existingTraining.Description = training.Description;
                existingTraining.CoverPicture = training.CoverPicture;
                existingTraining.Duration = training.Duration;
                existingTraining.CourseCode = training.CourseCode;
                existingTraining.Ratings = training.Ratings;
                existingTraining.UpdatedTime = training.UpdatedTime;
                existingTraining.UpdatedBy = training.UpdatedBy;
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteTraining(Training training)
        {
            var existingTraining = this.GetDbSet<Training>().FirstOrDefault(x => x.Id == training.Id);
            if (existingTraining != null)
            {
                this.GetDbSet<Training>().Remove(existingTraining);
                UnitOfWork.SaveChanges();
            }

        }
    }

}
