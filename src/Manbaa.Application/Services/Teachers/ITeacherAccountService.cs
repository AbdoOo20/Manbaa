using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Teachers;

namespace Manbaa.Application.Services.Teachers;

/// <summary>Creates a teacher together with the teacher's own login. Needs ASP.NET Identity, so it lives in Infrastructure.</summary>
public interface ITeacherAccountService
{
    Task<Result<TeacherDetailsDto>> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken = default);
}
