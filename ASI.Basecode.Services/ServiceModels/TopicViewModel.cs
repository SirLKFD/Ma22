using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASI.Basecode.Data.Models;

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
    public DateTime UpdatedTime { get; set; }

    public string AccountFirstName { get; set; }
    public string AccountLastName { get; set; }

    public int Id { get; set; }

    public int MediaCount { get; set; }
    
    public List<TopicMediaViewModel> Media { get; set; }
}
