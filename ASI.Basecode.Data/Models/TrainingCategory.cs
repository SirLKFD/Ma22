using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class TrainingCategory
{
    public string CategoryName { get; set; }

    public int AccountId { get; set; }

    public string Description { get; set; }

    public string CoverPicture { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public int Id { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();
}
