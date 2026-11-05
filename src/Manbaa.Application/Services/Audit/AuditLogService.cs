using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Audit;

using Manbaa.Domain.Entities;

namespace Manbaa.Application.Services.Audit;

public class AuditLogService(IUnitOfWork unitOfWork, IUserAppService currentUser) : IAuditLogService
{
    public Task<Result<PagedResult<AuditLogDto>>> GetForTeacherAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var teacherId = currentUser.RequiredTeacherId;

        return SearchAsync(unitOfWork.Repository<AuditLog>().Query().Where(log => log.TeacherId == teacherId), query, cancellationToken);
    }

    public Task<Result<PagedResult<AuditLogDto>>> GetForAdminAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var logs = unitOfWork.Repository<AuditLog>().Query();

        if (query.TeacherId is { } teacherId)
        {
            logs = logs.Where(log => log.TeacherId == teacherId);
        }

        return SearchAsync(logs, query, cancellationToken);
    }

    private static async Task<Result<PagedResult<AuditLogDto>>> SearchAsync(IQueryable<AuditLog> logs, AuditLogQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId is { } userId)
        {
            logs = logs.Where(log => log.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            logs = logs.Where(log => log.EntityName == query.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            logs = logs.Where(log => log.EntityId == query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logs = logs.Where(log => log.Action == query.Action);
        }

        if (query.From is { } from)
        {
            logs = logs.Where(log => log.CreatedAt >= from);
        }

        if (query.To is { } to)
        {
            var end = to.Date.AddDays(1);

            logs = logs.Where(log => log.CreatedAt < end);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            logs = logs.Where(log => (log.UserName != null && log.UserName.Contains(search)) || (log.NewValues != null && log.NewValues.Contains(search)) || (log.OldValues != null && log.OldValues.Contains(search)));
        }

        var page = await logs
            .OrderByDescending(log => log.Id)
            .Select(log => new AuditLogDto(log.Id, log.TeacherId, log.UserId, log.UserName, log.UserRole, log.Action, log.EntityName, log.EntityId, log.OldValues, log.NewValues, log.IpAddress, log.UserAgent, log.CreatedAt))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<AuditLogDto>>.Ok(page);
    }
}
