using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserProfileViewModel
    {
        public string EmailId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Contact { get; set; }

        public DateOnly? Birthdate { get; set; }

        public string ProfilePicture {get;set;}

        public DateTime CreatedTime {get;set;}

        public int Id {get;set;}

    }
}
