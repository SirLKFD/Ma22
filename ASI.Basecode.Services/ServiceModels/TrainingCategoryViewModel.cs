using System;
using ASI.Basecode.Data.Models;
using System.ComponentModel.DataAnnotations;
namespace ASI.Basecode.Services.ServiceModels;

public class TrainingCategoryViewModel
{

    [Required(ErrorMessage = "Category name is required.")]
    public string CategoryName { get; set; }

    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string? Description { get; set; }

    public string? CoverPicture { get; set; }
    public int Id { get; set; }
    public DateTime UpdatedTime { get; set; }

    public string AccountFirstName { get; set; }

    public string AccountLastName { get; set; }

}
