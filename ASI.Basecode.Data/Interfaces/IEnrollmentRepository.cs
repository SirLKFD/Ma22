using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IEnrollmentRepository
    {
        void Add(Enrollment enrollment);
        bool IsEnrolled(int userId, int trainingId);
        List<Enrollment> GetByUserId(int userId);
        List<Enrollment> GetByTrainingId(int trainingId);
    }
}