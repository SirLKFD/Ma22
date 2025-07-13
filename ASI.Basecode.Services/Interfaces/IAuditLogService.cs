using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAuditLogService
    {
    List<AuditLogViewModel> GetRecentLogs(string actionType, int count = 5);
    void LogAction(string entity, string actionType, int entityId, int? userId, string entityName);
    }
}
