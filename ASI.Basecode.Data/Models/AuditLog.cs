using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class AuditLog
{
    public int Id { get; set; }

    public string Entity { get; set; }

    public string EntityName {get;set;}

    public string ActionType { get; set; }

    public int? EntityId { get; set; }

    public int? AccountId { get; set; }

    public DateTime TimeStamp { get; set; }

    public virtual Account Account { get; set; }
}
