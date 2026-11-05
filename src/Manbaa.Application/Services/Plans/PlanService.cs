using Microsoft.EntityFrameworkCore;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Plans;

using Manbaa.Domain.Entities;

namespace Manbaa.Application.Services.Plans;

public class PlanService(IUnitOfWork unitOfWork) : IPlanService
{
    public async Task<Result<IReadOnlyList<PlanDto>>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var plans = unitOfWork.Repository<Plan>().Query();

        if (activeOnly)
        {
            plans = plans.Where(plan => plan.IsActive);
        }

        var items = await Project(plans.OrderBy(plan => plan.SortOrder).ThenBy(plan => plan.MonthlyPrice)).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PlanDto>>.Ok(items);
    }

    public async Task<Result<PlanDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await Project(unitOfWork.Repository<Plan>().Query().Where(item => item.Id == id)).FirstOrDefaultAsync(cancellationToken);

        return plan is null ? Result<PlanDto>.NotFound("الباقة مش موجودة.") : Result<PlanDto>.Ok(plan);
    }

    public async Task<Result<PlanDto>> CreateAsync(SavePlanRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await unitOfWork.Repository<Plan>().AnyAsync(plan => plan.Name == name, cancellationToken))
        {
            return Result<PlanDto>.Conflict("فيه باقة بنفس الاسم.");
        }

        var plan = new Plan();

        Apply(plan, request);

        await unitOfWork.Repository<Plan>().AddAsync(plan, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(plan.Id, cancellationToken);

        return Result<PlanDto>.Created(created.Data!, "الباقة اتضافت.");
    }

    public async Task<Result<PlanDto>> UpdateAsync(int id, SavePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await unitOfWork.Repository<Plan>().GetByIdAsync(id, cancellationToken);

        if (plan is null)
        {
            return Result<PlanDto>.NotFound("الباقة مش موجودة.");
        }

        var name = request.Name.Trim();

        if (await unitOfWork.Repository<Plan>().AnyAsync(item => item.Id != id && item.Name == name, cancellationToken))
        {
            return Result<PlanDto>.Conflict("فيه باقة بنفس الاسم.");
        }

        Apply(plan, request);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await unitOfWork.Repository<Plan>().GetByIdAsync(id, cancellationToken);

        if (plan is null)
        {
            return Result.NotFound("الباقة مش موجودة.");
        }

        if (await unitOfWork.Repository<Teacher>().AnyAsync(teacher => teacher.PlanId == id, cancellationToken))
        {
            return Result.Conflict("فيه مدرسين مشتركين في الباقة دي. وقّفها بدل ما تمسحها.");
        }

        unitOfWork.Repository<Plan>().Remove(plan);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok("الباقة اتمسحت.");
    }

    private IQueryable<PlanDto> Project(IQueryable<Plan> plans)
    {
        var teachers = unitOfWork.Repository<Teacher>().Query();

        return plans.Select(plan => new PlanDto(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.MonthlyPrice,
            plan.MaxStudents,
            plan.MaxSecretaries,
            plan.MaxStorageGb,
            plan.IsActive,
            plan.SortOrder,
            teachers.Count(teacher => teacher.PlanId == plan.Id)));
    }

    private static void Apply(Plan plan, SavePlanRequest request)
    {
        plan.Name = request.Name.Trim();

        plan.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        plan.MonthlyPrice = request.MonthlyPrice;

        plan.MaxStudents = request.MaxStudents;

        plan.MaxSecretaries = request.MaxSecretaries;

        plan.MaxStorageGb = request.MaxStorageGb;

        plan.IsActive = request.IsActive;

        plan.SortOrder = request.SortOrder;
    }
}
