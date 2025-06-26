using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Topic
{
    public int Id { get; set; }

    public string TopicName { get; set; }

    public int AccountId { get; set; }

    public int TrainingId { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<TopicMedium> TopicMedia { get; set; } = new List<TopicMedium>();

    public virtual Training Training { get; set; }
}
