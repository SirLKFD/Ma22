using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ITrainingRepository _trainingRepository;
        private readonly IMapper _mapper;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ITrainingRepository trainingRepository,
            IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _trainingRepository = trainingRepository;
            _mapper = mapper;
        }

        public void EnrollUser(int userId, int trainingId)
        {
            if (!_enrollmentRepository.IsEnrolled(userId, trainingId))
            {
                var enrollment = new Enrollment
                {
                    AccountId = userId,
                    TrainingId = trainingId,
                    EnrolledAt = DateTime.Now
                };
                _enrollmentRepository.Add(enrollment);
            }
        }

        public bool IsUserEnrolled(int userId, int trainingId)
        {
            return _enrollmentRepository.IsEnrolled(userId, trainingId);
        }

        public List<TrainingViewModel> GetEnrolledTrainings(int userId)
        {
            var enrollments = _enrollmentRepository.GetByUserId(userId);
            var trainingIds = enrollments.Select(e => e.TrainingId).ToList();
            var trainings = _trainingRepository.GetTrainings()
                .Where(t => trainingIds.Contains(t.Id))
                .ToList();
            return _mapper.Map<List<TrainingViewModel>>(trainings);
        }
    }
}
