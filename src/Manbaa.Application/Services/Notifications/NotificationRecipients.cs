using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Manbaa.Application.Abstractions;

using Manbaa.Domain.Constants;

using Manbaa.Domain.Entities;

namespace Manbaa.Application.Services.Notifications;

public class NotificationRecipients(IUnitOfWork unitOfWork) : INotificationRecipients
{
    public Task<IReadOnlyList<int>> SystemAdminsAsync(CancellationToken cancellationToken = default) => UsersInRoleAsync(RoleNames.SystemAdmin, null, cancellationToken);

    public Task<IReadOnlyList<int>> TeacherOwnersAsync(int teacherId, CancellationToken cancellationToken = default) => UsersInRoleAsync(RoleNames.Teacher, teacherId, cancellationToken);

    private async Task<IReadOnlyList<int>> UsersInRoleAsync(string roleName, int? teacherId, CancellationToken cancellationToken)
    {
        var users = unitOfWork.Repository<User>().Query().Where(user => user.IsActive);

        users = teacherId is null ? users.Where(user => user.TeacherId == null) : users.Where(user => user.TeacherId == teacherId);

        var ids = await (
            from user in users
            join userRole in unitOfWork.Repository<IdentityUserRole<int>>().Query() on user.Id equals userRole.UserId
            join role in unitOfWork.Repository<Role>().Query() on userRole.RoleId equals role.Id
            where role.Name == roleName
            select user.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids;
    }
}
