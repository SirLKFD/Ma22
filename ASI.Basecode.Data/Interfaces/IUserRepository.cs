using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<Account> GetUsers();
        Account GetUserById(int id);
        bool UserExists(string emailId);
        bool UserExists(string emailId, int excludeId);
        void AddUser(Account user);
        void UpdateUser(Account user);
        void DeleteUser(Account user);
    }
}
