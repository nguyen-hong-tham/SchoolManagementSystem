using System;

namespace Shared.Events
{
    // sự kiện thêm mới user
    public class UserCreatedEvent
    {
        public Guid Id { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // sự kiện cập nhật user
    public class UserUpdatedEvent
    {
        public Guid Id { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // sự kiện xóa user
    public class UserDeletedEvent
    {
        public Guid Id { get; set; }
    }
}
