using System;

namespace Shared.Events;

public interface UserCreatedEvent
{
    Guid Id { get; }
    string UserCode { get; }
    string FullName { get; }
    string Role { get; }
}

public interface UserUpdatedEvent
{
    Guid Id { get; }
    string UserCode { get; }
    string FullName { get; }
    string Role { get; }
}

public interface UserDeletedEvent
{
    Guid Id { get; }
}
