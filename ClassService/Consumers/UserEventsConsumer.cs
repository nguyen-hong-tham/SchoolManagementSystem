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
            if (!existing)
            {
                var cachedUser = new CachedUser
                {
                    Id = data.Id,
                    UserCode = data.UserCode,
                    FullName = data.FullName,
                    Role = data.Role,
                    StudentStatus = data.StudentStatus,
                    LastUpdated = System.DateTime.UtcNow,
                };
                _dbContext.CachedUsers.Add(cachedUser);
            }

            if (data.Role == "Student" && data.ClassId != null && data.ClassId != Guid.Empty)
            {
                var targetClass = await _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == data.ClassId.Value);
                if (targetClass != null)
                {
                    // Check if student is already in this class
                    var studentInClassExists = await _dbContext.StudentClasses.AnyAsync(sc => sc.StudentId == data.Id && sc.ClassId == data.ClassId.Value && sc.IsCurrent);
                    if (!studentInClassExists)
                    {
                        var studentClass = new StudentClass
                        {
                            Id = Guid.NewGuid(),
                            StudentId = data.Id,
                            ClassId = data.ClassId.Value,
                            SchoolYear = targetClass.SchoolYear,
                            AssignedDate = System.DateTime.UtcNow,
                            IsCurrent = true,
                        };
                        _dbContext.StudentClasses.Add(studentClass);
                    }
                }
            }

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
                user.StudentStatus = data.StudentStatus;
                user.LastUpdated = System.DateTime.UtcNow;
            }

            if (data.Role == "Student")
            {
                if (data.StudentStatus == "Graduated")
                {
                    var currentAssignment = await _dbContext.StudentClasses.FirstOrDefaultAsync(sc => sc.StudentId == data.Id && sc.IsCurrent);
                    if (currentAssignment != null)
                    {
                        currentAssignment.IsCurrent = false;
                        currentAssignment.LeftDate = System.DateTime.UtcNow;
                        currentAssignment.PromotionStatus = "Graduated";
                    }
                }
                else
                {
                    var currentAssignment = await _dbContext.StudentClasses.FirstOrDefaultAsync(sc => sc.StudentId == data.Id && sc.IsCurrent);
                    var newClassId = data.ClassId;

                    if (currentAssignment == null || currentAssignment.ClassId != newClassId)
                    {
                        if (currentAssignment != null)
                        {
                            currentAssignment.IsCurrent = false;
                            currentAssignment.LeftDate = System.DateTime.UtcNow;
                            currentAssignment.PromotionStatus = "Transferred";
                        }

                        if (newClassId != null && newClassId != Guid.Empty)
                        {
                            var targetClass = await _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == newClassId.Value);
                            if (targetClass != null)
                            {
                                var newAssignment = new StudentClass
                                {
                                    Id = Guid.NewGuid(),
                                    StudentId = data.Id,
                                    ClassId = newClassId.Value,
                                    SchoolYear = targetClass.SchoolYear,
                                    AssignedDate = System.DateTime.UtcNow,
                                    IsCurrent = true,
                                };
                                _dbContext.StudentClasses.Add(newAssignment);
                            }
                        }
                    }
                }
            }
            else
            {
                var currentAssignment = await _dbContext.StudentClasses.FirstOrDefaultAsync(sc => sc.StudentId == data.Id && sc.IsCurrent);
                if (currentAssignment != null)
                {
                    currentAssignment.IsCurrent = false;
                    currentAssignment.LeftDate = System.DateTime.UtcNow;
                    currentAssignment.PromotionStatus = "Transferred";
                }
            }

            await _dbContext.SaveChangesAsync();
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
