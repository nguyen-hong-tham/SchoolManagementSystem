namespace ClassService.Entities;

public class HomeroomAssignment
{
    //Phân công giáo viên chủ nhiệm.
    public Guid Id { get; set; }

    public Guid TeacherId { get; set; }

    public Guid ClassId { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}