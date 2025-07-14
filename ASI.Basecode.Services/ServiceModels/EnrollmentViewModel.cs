using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class EnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public int TrainingId { get; set; }
        public int AccountId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string FullName => $"{UserFirstName} {UserLastName}";
    }
} 