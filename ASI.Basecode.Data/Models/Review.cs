using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public string UserReview { get; set; }

    public int ReviewScore { get; set; }

    public int TrainingId { get; set; }

    public int AccountId { get; set; }

    public string Title { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public virtual Account Account { get; set; }

    public virtual Training Training { get; set; }
}
