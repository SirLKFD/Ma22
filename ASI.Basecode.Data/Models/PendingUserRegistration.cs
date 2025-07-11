using System;

namespace ASI.Basecode.Data.Models
{
    public class PendingUserRegistration
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Contact { get; set; }
        public string PasswordHash { get; set; }
        public string VerificationToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsVerified { get; set; }
    }
} 