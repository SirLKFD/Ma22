using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEnrollmentService
    {
        void EnrollUser(int userId, int trainingId);
        bool IsUserEnrolled(int userId, int trainingId);
        List<TrainingViewModel> GetEnrolledTrainings(int userId);
    }
}
