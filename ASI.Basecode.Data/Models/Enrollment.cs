using System;

namespace ASI.Basecode.Data.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int AccountId { get; set; } 
        public int TrainingId { get; set; }
        public DateTime EnrolledAt { get; set; } 
    }
}
