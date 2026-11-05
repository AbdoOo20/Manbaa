using Manbaa.Application.DTOs.Teachers;

namespace Manbaa.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    int TotalTeachers,
    int TrialTeachers,
    int ActiveTeachers,
    int ExpiringSoonTeachers,
    int InGraceTeachers,
    int ExpiredTeachers,
    int SuspendedTeachers,
    int NewTeachersThisMonth,
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    IReadOnlyList<MonthlyRevenueDto> RevenueByMonth,
    IReadOnlyList<TeacherListItemDto> ExpiringSoon);
