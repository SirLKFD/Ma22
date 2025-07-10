using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly AsiBasecodeDBContext _context;

        public EnrollmentRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();
        }

        public bool IsEnrolled(int userId, int trainingId)
        {
            return _context.Enrollments.Any(e => e.AccountId == userId && e.TrainingId == trainingId);
        }

        public List<Enrollment> GetByUserId(int userId)
        {
            return _context.Enrollments
                .Where(e => e.AccountId == userId)
                .ToList();
        }

        public List<Enrollment> GetByTrainingId(int trainingId)
        {
            return _context.Enrollments
                .Where(e => e.TrainingId == trainingId)
                .ToList();
        }

        public List<Enrollment> GetByTrainingIds(List<int> trainingIds)
        {
            return _context.Enrollments
                .Where(e => trainingIds.Contains(e.TrainingId))
                .ToList();
        }
    }
}
