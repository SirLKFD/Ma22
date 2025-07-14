using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITrainingService _trainingService;

    public AuditLogService(IAuditLogRepository repository, IHttpContextAccessor httpContextAccessor, ITrainingService trainingService) 
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _trainingService = trainingService;
    }

    public List<AuditLogViewModel> GetRecentLogs(string actionType, int count = 5)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        int? accountRole = session?.GetInt32("AccountRole");
        int? accountId = session?.GetInt32("AccountId");

        var logsQuery = _repository.GetAuditLogs()
            .Where(l => l.ActionType == actionType);

        if (accountRole == 2)
        {
            // SuperAdmin: show all logs
        }
        else if (accountRole == 0)
        {
            // Admin: show their own logs plus enrollment/review logs for their trainings
            var adminTrainingNames = _trainingService.GetAllTrainings()
                .Where(t => t.AccountId == accountId)
                .Select(t => t.TrainingName)
                .ToList();

            logsQuery = logsQuery.Where(l => 
                l.AccountId == accountId || // Their own logs
                (l.Entity == "Enroll" && l.ActionType == "Create" && 
                 adminTrainingNames.Any(name => l.EntityName.Contains(name))) || // Enrollment logs for their trainings
                (l.Entity == "Review" && l.ActionType == "Create" && 
                 adminTrainingNames.Any(name => l.EntityName.Contains(name))) // Review logs for their trainings
            );
        }
        else if (accountRole == 1)
        {
            // User: show only logs for this user
            logsQuery = logsQuery.Where(l => l.AccountId == accountId);
        }
        else
        {
            // Unknown role: return empty
            return new List<AuditLogViewModel>();
        }

        var logs = logsQuery
            .OrderByDescending(l => l.TimeStamp)
            .Take(count)
            .ToList();

        var result = new List<AuditLogViewModel>();

        foreach (var log in logs)
        {
            result.Add(new AuditLogViewModel
            {
                Id = log.Id,
                Entity = log.Entity,
                ActionType = log.ActionType,
                AccountName = log.AccountName,
                AccountId = log.AccountId,
                EntityName = log.EntityName,
                TimeStamp = log.TimeStamp
            });
        }

        return result;
    }

    public void LogAction(string entity, string actionType, string accountName, int accountId, string entityName)
    {
        try
        {
            var log = new AuditLog
            {
                Entity = entity,
                EntityName = entityName,
                ActionType = actionType,
                AccountName = accountName,
                AccountId = accountId,
                TimeStamp = DateTime.UtcNow.AddHours(8)
            };
            _repository.AddAuditLog(log);
            Console.WriteLine($"[AuditLogService] Successfully logged action: {actionType} on {entity} by {accountName} (ID: {accountId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuditLogService] Error logging action: {ex.Message}");
            Console.WriteLine($"[AuditLogService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
}
