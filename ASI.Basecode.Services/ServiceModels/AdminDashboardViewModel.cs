using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AdminDashboardViewModel
    {
        public int EnrolleeCount {get;set;}

        public int CategoryCount {get;set;}

        public int TrainingCount {get;set;}

        public int? MostEnrolledTrainingId { get; set; }
        public string MostEnrolledTrainingName { get; set; }
        public int? LeastEnrolledTrainingId { get; set; }
        public string LeastEnrolledTrainingName { get; set; }

        public List<AuditLogViewModel> RecentCreatedLogs { get; set; }
        public List<AuditLogViewModel> RecentUpdatedLogs { get; set; }
        public List<AuditLogViewModel> RecentDeletedLogs { get; set; }
    }
}
