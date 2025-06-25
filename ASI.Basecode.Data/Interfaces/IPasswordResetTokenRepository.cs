using ASI.Basecode.Data.Models;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        IQueryable<PasswordResetToken> GetTokens();
        PasswordResetToken GetByToken(string token);
        void AddToken(PasswordResetToken token);
        void UpdateToken(PasswordResetToken token);
        void DeleteToken(PasswordResetToken token);
    }
} 