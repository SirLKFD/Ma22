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

        public bool UserExists(string emailId)
        {
            return this.GetDbSet<Account>().Any(x => x.EmailId == emailId);
        }

        public void AddUser(Account user)
        {
            this.GetDbSet<Account>().Add(user);
            UnitOfWork.SaveChanges();
        }

    }
}
