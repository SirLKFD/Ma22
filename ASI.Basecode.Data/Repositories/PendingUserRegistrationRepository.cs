using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;

using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PendingUserRegistrationRepository : IPendingUserRegistrationRepository
    {
        private readonly AsiBasecodeDBContext _context;
        public PendingUserRegistrationRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        public void Add(PendingUserRegistration pendingUser)
        {
            _context.PendingUserRegistrations.Add(pendingUser);
        }

        public PendingUserRegistration GetByToken(string token)
        {
            return _context.PendingUserRegistrations.FirstOrDefault(x => x.VerificationToken == token);
        }

        public PendingUserRegistration GetByEmail(string email)
        {
            return _context.PendingUserRegistrations.FirstOrDefault(x => x.EmailId == email);
        }

        public void Update(PendingUserRegistration pendingUser)
        {
            _context.PendingUserRegistrations.Update(pendingUser);
        }

        public void Delete(PendingUserRegistration pendingUser)
        {
            _context.PendingUserRegistrations.Remove(pendingUser);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
} 