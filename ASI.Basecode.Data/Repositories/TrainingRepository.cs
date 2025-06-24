using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class TrainingRepository : BaseRepository, ITrainingRepository
    {
        public TrainingRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Training> GetTrainings()
        {
            return this.GetDbSet<Training>();
        }

        public bool TrainingExists(string name)
        {
            return this.GetDbSet<Training>().Any(x => x.TrainingName == name);
        }

         public void AddTraining(Training training)
        {
            this.GetDbSet<Training>().Add(training);
            UnitOfWork.SaveChanges();
        }
    }
}
