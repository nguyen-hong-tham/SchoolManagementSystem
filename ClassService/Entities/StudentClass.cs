namespace ClassService.Entities;

public class StudentClass
{
    //Danh sách học sinh thuộc lớp theo năm học.
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ClassId { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    public bool IsCurrent { get; set; } = true;

    public DateTime? LeftDate { get; set; }

    public string? PromotionStatus { get; set; }
}