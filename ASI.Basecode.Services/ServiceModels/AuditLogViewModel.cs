using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AuditLogViewModel
    {
        public string Entity { get; set; }
        public string ActionType { get; set; }
        public int EntityId { get; set; }

        public string EntityName {get;set;}
        public int? AccountId { get; set; }

        public string AccountName{get; set;}
        public DateTime TimeStamp { get; set; }

    }
}
