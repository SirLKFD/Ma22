using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
                .Include(t => t.SkillLevelNavigation)
                .Include(t => t.TrainingCategory)
                .Where(t => trainingIds.Contains(t.Id))
                .ToList();
            return trainings.Select(t => new TrainingViewModel
            {
                Id = t.Id,
                Ratings = t.Ratings,
                AccountId = t.AccountId,
                TrainingName = t.TrainingName,
                TrainingCategoryId = t.TrainingCategoryId,
                TrainingCategoryName = t.TrainingCategory.CategoryName,
                SkillLevel = t.SkillLevel,
                SkillLevelName = t.SkillLevelNavigation.SkillLevel1,
                Description = t.Description,
                CoverPicture = t.CoverPicture,
                Duration = t.Duration,
                CourseCode = t.CourseCode,
                UpdatedTime = t.UpdatedTime,
                AccountFirstName = t.Account.FirstName,
                AccountLastName = t.Account.LastName
            }).ToList();
        }

        public int GetEnrollmentCount(int trainingId)
        {
            var enrollments = _enrollmentRepository.GetByTrainingId(trainingId);
            return enrollments.Count;
        }
    }
}
