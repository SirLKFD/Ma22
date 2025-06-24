using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string FullName { get; set; }  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Contact { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime? Birthdate { get; set; }
        public int Role { get; set; } // 0 = Admin, 1 = Guest
    }
}
