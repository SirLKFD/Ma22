using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string EmailId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact must start with 09 and be 11 digits.")]
        public string Contact { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birthdate")]
        public DateOnly? Birthdate { get; set; }

        public string Password { get; set; } // Optional for edit

        [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public int Role { get; set; }

        public string ProfilePicture { get; set; }

        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
    }
} 