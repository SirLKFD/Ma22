using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Account
{
    public int Id { get; set; }

    public string EmailId { get; set; }

    public string Password { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Contact { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string ProfilePicture { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedTime { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime UpdatedTime { get; set; }

    public int Role { get; set; }

    public virtual Role RoleNavigation { get; set; }

    public virtual ICollection<TopicMedium> TopicMedia { get; set; } = new List<TopicMedium>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();

    public virtual ICollection<TrainingCategory> TrainingCategories { get; set; } = new List<TrainingCategory>();
}
