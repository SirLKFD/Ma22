using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAuditLogRepository
    {
        IQueryable<AuditLog> GetAuditLogs();
        void AddAuditLog(AuditLog log);
    }
}