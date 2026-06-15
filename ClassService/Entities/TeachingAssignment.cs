namespace ClassService.Entities;

public class TeachingAssignment
{
    //Phân công giảng dạy của giáo viên cho môn học và lớp học.
    public Guid Id { get; set; }

    public Guid TeacherId { get; set; }

    public Guid SubjectId { get; set; }

    public Guid ClassId { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}