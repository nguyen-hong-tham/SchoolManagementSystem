using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScoreService.Data;
using ScoreService.Entities;
using Shared.Events;

namespace ScoreService.Consumers;

//nhiệm vụ là lắng nghe sự kiện UserCreatedEvent từ UserService rồi lưu một bản sao user vào database của ScoreService

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{ //IConsumer là một interface trong thư viện MassTransit (dùng với RabbitMQ hoặc các message broker khác). Nó định nghĩa rằng một class sẽ nhận và xử lý (consume) các message thuộc một kiểu cụ thể
    private readonly ScoreDbContext _db;

    public UserCreatedConsumer(ScoreDbContext db) => _db = db; // để consumer có thể thao tác vs db

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    // B1: UserService tạo User và lưu vào User DB
    // B2: UserService publish UserCreatedEvent lên RabbitMQ
    // B3: RabbitMQ chuyển event đến ScoreService
    // B4: MassTransit tự động gọi UserCreatedConsumer.Consume()
    // B5: Lấy dữ liệu user từ context.Message
    // B6: Kiểm tra user đã tồn tại trong CachedUsers chưa
    // B7: Nếu chưa có thì tạo CachedUser
    // B8: Thêm vào DbContext và lưu xuống Score DB
    {
        var data = context.Message; // lấy dữ liệu sự kiện
        var exists = await _db.CachedUsers.AnyAsync(u => u.Id == data.Id); // kiểm tra tồn tại sau khi lấy
        if (exists)
            return;

        var cached = new CachedUser
        {
            Id = data.Id,
            UserCode = data.UserCode,
            FullName = data.FullName,
            Role = data.Role,
            LastUpdated = DateTime.UtcNow,
        };
        _db.CachedUsers.Add(cached);
        await _db.SaveChangesAsync();
    }
}

public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
{
    private readonly ScoreDbContext _db;

    public UserUpdatedConsumer(ScoreDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var data = context.Message;
        var user = await _db.CachedUsers.FindAsync(data.Id);
        if (user != null)
        {
            user.UserCode = data.UserCode;
            user.FullName = data.FullName;
            user.Role = data.Role;
            user.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}

public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
{
    private readonly ScoreDbContext _db;

    public UserDeletedConsumer(ScoreDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var data = context.Message;
        var user = await _db.CachedUsers.FindAsync(data.Id);
        if (user != null)
        {
            _db.CachedUsers.Remove(user);
            await _db.SaveChangesAsync();
        }
    }
}
