using System;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.ServiceModels;

public class TrainingCategoryViewModel
{
    public int Id { get; set; }

    public string CategoryName { get; set; }

    public int AccountId { get; set; }

    public string? Description { get; set; }

    public string? CoverPicture { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public virtual Account Account { get; set; }
}
