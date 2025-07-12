using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPendingUserRegistrationRepository
    {
        void Add(PendingUserRegistration pendingUser);
        PendingUserRegistration GetByToken(string token);
        PendingUserRegistration GetByEmail(string email);
        void Update(PendingUserRegistration pendingUser);
        void Delete(PendingUserRegistration pendingUser);
        void Save();
    }
} 