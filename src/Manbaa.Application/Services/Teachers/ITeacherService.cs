using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Teachers;

namespace Manbaa.Application.Services.Teachers;

/// <summary>The system admin's management of teachers. Creating a teacher with a login is in <see cref="ITeacherAccountService" />.</summary>
public interface ITeacherService
{
    Task<Result<PagedResult<TeacherListItemDto>>> GetAllAsync(TeacherQuery query, CancellationToken cancellationToken = default);

    Task<Result<TeacherDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<TeacherDetailsDto>> UpdateAsync(int id, UpdateTeacherRequest request, CancellationToken cancellationToken = default);

    Task<Result> SuspendAsync(int id, SuspendTeacherRequest request, CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default);
}
