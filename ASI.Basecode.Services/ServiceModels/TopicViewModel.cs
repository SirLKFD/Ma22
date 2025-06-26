using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels;

public class TopicViewModel
{
    [Required(ErrorMessage = "Topic name is required.")]
    public string TopicName { get; set; }

    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Training ID is required.")]
    public int TrainingId { get; set; }

    public string? Description { get; set; }


}
