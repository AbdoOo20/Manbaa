using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Auth;

/// <summary><see cref="Login" /> is the email or the phone number.</summary>
public record LoginRequest(
    [Required(ErrorMessage = "اكتب الإيميل أو رقم الموبايل."), MaxLength(256)] string Login,
    [Required(ErrorMessage = "اكتب كلمة السر."), MaxLength(128)] string Password);
