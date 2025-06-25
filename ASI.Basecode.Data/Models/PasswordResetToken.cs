using System;

namespace ASI.Basecode.Data.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; }
        public virtual Account Account { get; set; }
    }
} 