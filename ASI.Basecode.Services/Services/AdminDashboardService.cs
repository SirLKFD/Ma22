using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly ITrainingCategoryRepository _trainingCategoryRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;

        public AdminDashboardService(
            ITrainingRepository trainingRepository,
            ITrainingCategoryRepository trainingCategoryRepository,
            IEnrollmentRepository enrollmentRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogService auditLogService,
            IMapper mapper)
        {
            _trainingRepository = trainingRepository;
            _trainingCategoryRepository = trainingCategoryRepository;
            _enrollmentRepository = enrollmentRepository;
            _httpContextAccessor = httpContextAccessor;
            _auditLogService = auditLogService;
            _mapper = mapper;
        }

        public AdminDashboardViewModel GetAdminDashboardInformation()
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");

            IQueryable<Training> trainingsQuery;
            IQueryable<TrainingCategory> categoriesQuery;

            if (accountRole.HasValue && accountRole == 0)
            {
                trainingsQuery = _trainingRepository.GetTrainings().Where(t => t.AccountId == accountId);
                categoriesQuery = _trainingCategoryRepository.GetTrainingCategories().Where(t => t.AccountId == accountId);
            }
            else
            {
                trainingsQuery = _trainingRepository.GetTrainings();
                categoriesQuery = _trainingCategoryRepository.GetTrainingCategories();
            }

            var trainings = trainingsQuery.ToList();
            var trainingIds = trainings.Select(t => t.Id).ToList();
            var categories = categoriesQuery.ToList();

            // Batch load all enrollments for these trainings
            var allEnrollments = _enrollmentRepository.GetByTrainingIds(trainingIds);

            // Group enrollments by TrainingId
            var enrollmentCounts = allEnrollments
                .GroupBy(e => e.TrainingId)
                .ToDictionary(g => g.Key, g => g.Count());

            int trainingCount = trainings.Count;
            int categoryCount = categories.Count;
            int enrolleeCount = allEnrollments.Count;

            int mostEnrolledCount = -1;
            int leastEnrolledCount = int.MaxValue;
            int? mostEnrolledId = null;
            string mostEnrolledName = null;
            int? leastEnrolledId = null;
            string leastEnrolledName = null;

            foreach (var training in trainings)
            {
                int count = enrollmentCounts.TryGetValue(training.Id, out var c) ? c : 0;
                if (count > mostEnrolledCount)
                {
                    mostEnrolledCount = count;
                    mostEnrolledId = training.Id;
                    mostEnrolledName = training.TrainingName;
                }
                if (count < leastEnrolledCount)
                {
                    leastEnrolledCount = count;
                    leastEnrolledId = training.Id;
                    leastEnrolledName = training.TrainingName;
                }
            }

            var model = new AdminDashboardViewModel
            {
                TrainingCount = trainingCount,
                CategoryCount = categoryCount,
                EnrolleeCount = enrolleeCount,
                MostEnrolledTrainingId = mostEnrolledId,
                MostEnrolledTrainingName = mostEnrolledName,
                LeastEnrolledTrainingId = leastEnrolledId,
                LeastEnrolledTrainingName = leastEnrolledName,
                RecentCreatedLogs = _auditLogService.GetRecentLogs("Create", 5),
                RecentUpdatedLogs = _auditLogService.GetRecentLogs("Update", 5),
                RecentDeletedLogs = _auditLogService.GetRecentLogs("Delete", 5)
            };

            return model;
        }

        public int GetEnrollmentCountForUser(int userId, int accountRole)
        {
            List<int> trainingIds;

            if (accountRole == 0)
            {
                // Admin: Only their own trainings
                trainingIds = _trainingRepository.GetTrainings()
                    .Where(t => t.AccountId == userId)
                    .Select(t => t.Id)
                    .ToList();
            }
            else if (accountRole == 2)
            {
                // SuperAdmin: All trainings
                trainingIds = _trainingRepository.GetTrainings()
                    .Select(t => t.Id)
                    .ToList();
            }
            else
            {
                trainingIds = new List<int>();
            }

            int totalEnrollments = 0;
            foreach (var trainingId in trainingIds)
            {
                totalEnrollments += _enrollmentRepository.GetByTrainingId(trainingId).Count;
            }
            return totalEnrollments;
        }
    }
}
