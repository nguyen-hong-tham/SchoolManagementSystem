namespace ClassService.DTOs.HomeroomAssignments;

public class AssignHomeroomDto
{
    public Guid id { get; set; } 
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate {get;set;}
}
