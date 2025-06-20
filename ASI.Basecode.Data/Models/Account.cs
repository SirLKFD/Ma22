using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models;

public partial class Account
{
    [Key]
    public int Id { get; set; }

    public string EmailId { get; set; }

    public string Password { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Contact { get; set; }

    public string ProfilePicture { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public int Role { get; set; }

    public virtual ICollection<TrainingCategory> TrainingCategories { get; set; } = new List<TrainingCategory>();
}
