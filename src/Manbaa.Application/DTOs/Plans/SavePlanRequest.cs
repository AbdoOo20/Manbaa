using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Plans;

public record SavePlanRequest(
    [Required(ErrorMessage = "اكتب اسم الباقة."), MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(0, 1_000_000, ErrorMessage = "السعر مش صحيح.")] decimal MonthlyPrice,
    [Range(1, 1_000_000, ErrorMessage = "عدد الطلاب مش صحيح.")] int MaxStudents,
    [Range(0, 1000, ErrorMessage = "عدد السكرتارية مش صحيح.")] int MaxSecretaries,
    [Range(0, 100_000, ErrorMessage = "المساحة مش صحيحة.")] int MaxStorageGb,
    bool IsActive,
    int SortOrder);
