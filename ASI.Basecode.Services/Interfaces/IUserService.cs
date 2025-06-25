using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string emailid, string password, ref Account user);
        void AddUser(UserViewModel model);
        void AddUser(AdminCreateUserViewModel model);
        List<UserListViewModel> GetAllUsers();
        UserEditViewModel GetUserById(int id);
        void UpdateUser(UserEditViewModel model);
        Account GetUserByEmail(string emailId);
        void DeleteUser(int userId);

    }
}
