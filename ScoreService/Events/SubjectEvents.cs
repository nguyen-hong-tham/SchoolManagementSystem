using System;

namespace Shared.Events;

public interface SubjectCreatedEvent
{
    Guid Id { get; }
    string Code { get; }
    string Name { get; }
    int GradeLevel { get; }
}

public interface SubjectUpdatedEvent
{
    Guid Id { get; }
    string Code { get; }
    string Name { get; }
    int GradeLevel { get; }
}

public interface SubjectDeletedEvent
{
    Guid Id { get; }
}
