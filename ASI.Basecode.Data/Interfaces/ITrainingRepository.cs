using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITrainingRepository
    {
        IQueryable<Training> GetTrainings();
        bool TrainingExists(string name);
        void AddTraining(Training training);
    }
}
