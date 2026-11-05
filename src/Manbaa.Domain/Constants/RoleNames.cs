namespace Manbaa.Domain.Constants;

public static class RoleNames
{
    public const string SystemAdmin = "SystemAdmin";

    public const string Teacher = "Teacher";

    public const string Secretary = "Secretary";

    public const string Student = "Student";

    public const string Parent = "Parent";

    public static readonly IReadOnlyList<string> All = [SystemAdmin, Teacher, Secretary, Student, Parent];
}
