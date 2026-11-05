namespace Manbaa.Application.DTOs.Plans;

public record PlanDto(int Id, string Name, string? Description, decimal MonthlyPrice, int MaxStudents, int MaxSecretaries, int MaxStorageGb, bool IsActive, int SortOrder, int TeachersCount);
