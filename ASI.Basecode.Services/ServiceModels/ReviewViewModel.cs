using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels;

public class ReviewViewModel
{
  
    public int ReviewId { get; set; }

    [Required(ErrorMessage = "Review is required.")]
    public string UserReview { get; set; }

    [Required(ErrorMessage = "ReviewScore is required.")]
    public int ReviewScore { get; set; }

    [Required(ErrorMessage = "TrainingId is required.")]
    public int TrainingId { get; set; }

    public int AccountId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; }

    public string AccountFirstName { get; set; }

    public string AccountLastName { get; set; }

    public string ProfilePicture { get; set; }

    public DateTime CreatedTime { get; set; }

}
