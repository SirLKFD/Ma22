using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class TopicMedium
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int TopicId { get; set; }

    public string MediaType { get; set; }

    public string MediaUrl { get; set; }

    public int AccountId { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public virtual Account Account { get; set; }

    public virtual Topic Topic { get; set; }
}
