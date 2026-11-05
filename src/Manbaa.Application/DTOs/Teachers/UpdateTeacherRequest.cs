using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Teachers;

public record UpdateTeacherRequest(
    [Required(ErrorMessage = "اكتب اسم المدرس."), MaxLength(150)] string FullName,
    [Required(ErrorMessage = "اكتب المادة."), MaxLength(100)] string Subject,
    [Required(ErrorMessage = "اكتب رقم الموبايل."), MaxLength(20)] string Phone,
    [EmailAddress(ErrorMessage = "الإيميل مش صحيح."), MaxLength(256)] string? Email,
    [MaxLength(1000)] string? Notes);
