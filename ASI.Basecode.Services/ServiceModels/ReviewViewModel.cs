using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels;

public class ReviewViewModel
{
  
    public int ReviewId { get; set; }
    public string UserReview { get; set; }

    [Required(ErrorMessage = "ReviewScore is required.") ]
    public int ReviewScore { get; set; }

    public int TrainingId { get; set; }

    public int AccountId { get; set; }

    public string Title { get; set; }

    public string AccountFirstName { get; set; }

    public string AccountLastName { get; set; }

    public DateTime CreatedTime { get; set; }

}
