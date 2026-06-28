using System;

namespace Shared.Events
{
    public class StudentPromotedEvent
    {
        public Guid StudentId { get; set; }
        public Guid? NewClassId { get; set; }
        public bool IsGraduating { get; set; }
    }
}
