using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AsiBasecodeDBContext _context;
        public AuditLogRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        public IQueryable<AuditLog> GetAuditLogs() => _context.AuditLogs;

        public void AddAuditLog(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
    }
}