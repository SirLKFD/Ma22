using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<Account> GetUsers()
        {
            return this.GetDbSet<Account>();
        }

        public Account GetUserById(int id)
        {
            return this.GetDbSet<Account>().FirstOrDefault(x => x.Id == id);
        }

        public bool UserExists(string emailId)
        {
            return this.GetDbSet<Account>().Any(x => x.EmailId == emailId);
        }

        public bool UserExists(string emailId, int excludeId)
        {
            return this.GetDbSet<Account>().Any(x => x.EmailId == emailId && x.Id != excludeId);
        }

        public void AddUser(Account user)
        {
            this.GetDbSet<Account>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(Account user)
        {
            var existingUser = this.GetDbSet<Account>().FirstOrDefault(x => x.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.EmailId = user.EmailId;
                existingUser.Contact = user.Contact;
                existingUser.Birthdate = user.Birthdate;
                existingUser.Role = user.Role;
                existingUser.ProfilePicture = user.ProfilePicture;
                existingUser.UpdatedTime = DateTime.Now;
                existingUser.UpdatedBy = System.Environment.UserName;

                // Only update password if provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                UnitOfWork.SaveChanges();
            }
        }
    }
}
