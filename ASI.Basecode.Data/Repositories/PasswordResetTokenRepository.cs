using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PasswordResetTokenRepository : BaseRepository, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IQueryable<PasswordResetToken> GetTokens()
        {
            return GetDbSet<PasswordResetToken>();
        }

        public PasswordResetToken GetByToken(string token)
        {
            return GetDbSet<PasswordResetToken>().FirstOrDefault(t => t.Token == token);
        }

        public void AddToken(PasswordResetToken token)
        {
            GetDbSet<PasswordResetToken>().Add(token);
        }

        public void UpdateToken(PasswordResetToken token)
        {
            SetEntityState(token, Microsoft.EntityFrameworkCore.EntityState.Modified);
        }

        public void DeleteToken(PasswordResetToken token)
        {
            GetDbSet<PasswordResetToken>().Remove(token);
        }
    }
} 