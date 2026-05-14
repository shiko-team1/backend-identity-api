namespace Application.Constants;

public static class RoleNames
{
    public const string Admin = "admin";
    public const string Instructor = "instructor";
    public const string Student = "student";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        Instructor,
        Student
    };
}