using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AdminCreateUserViewModel
    {
        public List<UserListViewModel> Users { get; set; } = new();

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string EmailId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        // Optional Contact (no [Required] attribute)
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Contact must start with 09 and be 11 digits.")]
        public string Contact { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birthdate")]
        public DateTime? Birthdate { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public int Role { get; set; }

        public string ProfilePicture { get; set; }

        // Pagination for the UserMaster Screen
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalGuests { get; set; }

    }
}
