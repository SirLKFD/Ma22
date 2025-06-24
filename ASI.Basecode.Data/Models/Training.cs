using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Training
{
    public int Id { get; set; }

    public string TrainingName { get; set; }

    public int AccountId { get; set; }

    public int TrainingCategoryId { get; set; }

    public int SkillLevel { get; set; }

    public string Description { get; set; }

    public string CoverPicture { get; set; }

    public int? Duration { get; set; }

    public string CourseCode { get; set; }

    public int Ratings { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public virtual Account Account { get; set; }

    public virtual Duration DurationNavigation { get; set; }

    public virtual SkillLevel SkillLevelNavigation { get; set; }

    public virtual TrainingCategory TrainingCategory { get; set; }
}
