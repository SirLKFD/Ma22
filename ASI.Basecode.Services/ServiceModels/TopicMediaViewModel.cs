using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels;

public class TopicMediaViewModel
{
    [Required(ErrorMessage = "Topic ID is required.")]
    public int TopicId { get; set; }

    [Required(ErrorMessage = "Media type is required.")]
    public string MediaType { get; set; }

    public string? Name { get; set; }

    [Required(ErrorMessage = "Media URL is required.")]
    public string MediaUrl { get; set; }

    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }

    public int Id {get;set;}

}
