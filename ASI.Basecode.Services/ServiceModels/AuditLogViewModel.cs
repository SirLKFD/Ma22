using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string Entity { get; set; }
        public string ActionType { get; set; }
        public string AccountName { get; set; }
        public int AccountId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string EntityName { get; set; }

    }
}
