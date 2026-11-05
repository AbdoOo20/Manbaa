using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Teachers;

public record SuspendTeacherRequest([Required(ErrorMessage = "اكتب سبب الإيقاف."), MaxLength(500)] string Reason);
