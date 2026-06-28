using System;
using System.Collections.Generic;

namespace ClassService.DTOs.StudentClasses;

public class PromoteBatchDto
{
    public List<Guid> StudentIds { get; set; } = new();
    public Guid? NewClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public bool IsGraduating { get; set; }
}
