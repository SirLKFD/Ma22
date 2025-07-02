using System;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels;

public class TrainingViewModel
{
    public int Id { get; set; }

    public int Ratings { get; set; }

    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Training name is required.")]
    public string TrainingName { get; set; }

    [Required(ErrorMessage = "Training category ID is required.")]
    public int TrainingCategoryId { get; set; }

    [Required(ErrorMessage = "Skill level is required.")]
    public int SkillLevel { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string? Description { get; set; }

    public string? CoverPicture { get; set; }

    public int? Duration { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    public string CourseCode { get; set; }
    public DateTime UpdatedTime { get; set; }

    public string AccountFirstName { get; set; }

    public string AccountLastName { get; set; }

}
