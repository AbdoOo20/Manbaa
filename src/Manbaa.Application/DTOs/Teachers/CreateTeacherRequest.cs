using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Teachers;

/// <summary>Creates the teacher (tenant) and the teacher's own login. Starts on a free trial.</summary>
public record CreateTeacherRequest(
    [Required(ErrorMessage = "اكتب اسم المدرس."), MaxLength(150)] string FullName,
    [Required(ErrorMessage = "اكتب المادة."), MaxLength(100)] string Subject,
    [Required(ErrorMessage = "اكتب رقم الموبايل."), MaxLength(20)] string Phone,
    [EmailAddress(ErrorMessage = "الإيميل مش صحيح."), MaxLength(256)] string? Email,
    [Required(ErrorMessage = "اكتب كلمة السر."), MinLength(8, ErrorMessage = "كلمة السر لازم تكون 8 حروف على الأقل."), MaxLength(128)] string Password,
    int? PlanId,
    [MaxLength(1000)] string? Notes);
