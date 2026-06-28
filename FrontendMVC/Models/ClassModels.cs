using System;

namespace FrontendMVC.Models;

public class ClassViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StudentClassViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public bool IsCurrent { get; set; }
}

public class HomeroomAssignmentViewModel
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class TeachingAssignmentViewModel
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class ScheduleViewModel
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; } // 2 -> 7
    public int Period { get; set; } // 1 -> 5
    public string Room { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
}

public class AssignStudentViewModel
{
    public Guid StudentId { get; set; }
}

public class AssignHomeroomViewModel
{
    public Guid TeacherId { get; set; }
}

public class AssignTeacherViewModel
{
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
}

public class CreateClassRequest
{
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public string SchoolYear { get; set; } = "2025-2026";
}

public class StudentAcademicYearViewModel
{
    public string SchoolYear { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public List<ScheduleViewModel> Schedules { get; set; } = new();
    public List<ScoreResponseViewModel> Scores { get; set; } = new();
    public bool IsCurrent { get; set; }
}

