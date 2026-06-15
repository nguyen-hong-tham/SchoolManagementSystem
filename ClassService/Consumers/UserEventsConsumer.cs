using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace ClassService.Consumers
{
    // consumer tiếp nhận tạo user
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        public UserCreatedConsumer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var data = context.Message;

            // Tránh trùng lặp nếu nhận lại tin nhắn cũ
            var existing = await _dbContext.CachedUsers.AnyAsync(u => u.Id == data.Id);
            if (existing)
                return;

            var cachedUser = new CachedUser
            {
                Id = data.Id,
                UserCode = data.UserCode,
                FullName = data.FullName,
                Role = data.Role,
                LastUpdated = System.DateTime.UtcNow,
            };

            _dbContext.CachedUsers.Add(cachedUser);
            await _dbContext.SaveChangesAsync();
        }
    }

    // consumer tiếp nhận cập nhật user
    public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        public UserUpdatedConsumer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            var data = context.Message;
            var user = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == data.Id);

            if (user != null)
            {
                user.UserCode = data.UserCode;
                user.FullName = data.FullName;
                user.Role = data.Role;
                user.LastUpdated = System.DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
            }
        }
    }

    // consumer tiếp nhận xóa user
    public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        public UserDeletedConsumer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var data = context.Message;
            var user = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == data.Id);

            if (user != null)
            {
                _dbContext.CachedUsers.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
