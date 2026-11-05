namespace Manbaa.Application.Settings;

public class SystemAdminSettings
{
    public const string SectionName = "SystemAdmin";

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string FullName { get; set; } = "مدير النظام";
}
