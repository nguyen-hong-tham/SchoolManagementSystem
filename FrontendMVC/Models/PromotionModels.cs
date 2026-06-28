using System;
using System.Collections.Generic;

namespace FrontendMVC.Models;

public class PromoteBatchViewModel
{
    public List<Guid> StudentIds { get; set; } = new();
    public Guid? NewClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public bool IsGraduating { get; set; }
}
