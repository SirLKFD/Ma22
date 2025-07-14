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
    private readonly ITrainingCategoryService _trainingCategoryService;
    private readonly ITrainingService _trainingService;
    private readonly ITopicService _topicService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    public AuditLogService(IAuditLogRepository repository, ITrainingCategoryService trainingCategoryService, ITrainingService trainingService, ITopicService topicService,IHttpContextAccessor httpContextAccessor, IUserService userService) 
    {
        _repository = repository;
        _trainingCategoryService = trainingCategoryService;
        _trainingService = trainingService;
        _topicService = topicService;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    public List<AuditLogViewModel> GetRecentLogs(string actionType, int count = 5)
    {
        var accountRole = _httpContextAccessor.HttpContext.Session.GetInt32("AccountRole");
        var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
        // Get the logs with account navigation property included
        var logsQuery = _repository.GetAuditLogs()
            .Include(l => l.Account)
            .Where(l => l.ActionType == actionType);

        if (accountRole == 0)
        {
            // For admins, include their own logs plus enrollment logs for trainings they created
            var adminTrainingIds = _trainingService.GetAllTrainings()
                .Where(t => t.AccountId == accountId)
                .Select(t => t.Id)
                .ToList();

            logsQuery = logsQuery.Where(l => l.AccountId == accountId || 
                                           (l.Entity == "User" && l.ActionType == "Create" && l.EntityName.StartsWith("Enrolled to") && 
                                            adminTrainingIds.Contains(l.EntityId)) || (l.Entity == "User" && l.ActionType == "Create" && l.EntityName.StartsWith("Reviewed") && 
                                            adminTrainingIds.Contains(l.EntityId)));
        }
        else if (accountRole == 2)
        {
            // Superadmin: no filter
        }
        else
        {
            // Other roles: return empty
            return new List<AuditLogViewModel>();
        }

        var logs = logsQuery
            .OrderByDescending(l => l.TimeStamp)
            .Take(count)
            .ToList();

        var result = new List<AuditLogViewModel>();

        foreach (var log in logs)
        {
            string accountName = log.Account != null
                ? $"{log.Account.FirstName} {log.Account.LastName}"
                : "";


            result.Add(new AuditLogViewModel
            {
                Entity = log.Entity,
                ActionType = log.ActionType,
                EntityId = log.EntityId,
                AccountId = log.AccountId,
                AccountName = accountName,
                EntityName = log.EntityName,
                TimeStamp = log.TimeStamp
            });
        }

        return result;
    }

    public void LogAction(string entity, string actionType, int entityId, int userId, string entityName)
    {
        try
        {
            var log = new AuditLog
            {
                Entity = entity,
                EntityName = entityName,
                ActionType = actionType,
                EntityId = entityId,
                AccountId = userId,
                TimeStamp = DateTime.UtcNow.AddHours(8)
            };
            
            _repository.AddAuditLog(log);
            
            // Log to console for debugging
            Console.WriteLine($"[AuditLogService] Successfully logged action: {actionType} on {entity} (ID: {entityId}) by user {userId}");
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.WriteLine($"[AuditLogService] Error logging action: {ex.Message}");
            Console.WriteLine($"[AuditLogService] Stack trace: {ex.StackTrace}");
            throw; // Re-throw to maintain the original behavior
        }
    }
}
}
