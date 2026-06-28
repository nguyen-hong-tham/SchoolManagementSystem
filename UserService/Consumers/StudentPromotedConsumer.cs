using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using UserService.Data;
using UserService.Entities;

namespace UserService.Consumers
{
    public class StudentPromotedConsumer : IConsumer<StudentPromotedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public StudentPromotedConsumer(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StudentPromotedEvent> context)
        {
            var data = context.Message;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == data.StudentId);

            if (user != null)
            {
                bool isChanged = false;

                if (data.IsGraduating)
                {
                    if (user.StudentStatus != StudentStatus.Graduated || user.ClassId != null)
                    {
                        user.StudentStatus = StudentStatus.Graduated;
                        user.ClassId = null;
                        isChanged = true;
                    }
                }
                else
                {
                    if (user.ClassId != data.NewClassId || user.StudentStatus != StudentStatus.Active)
                    {
                        user.ClassId = data.NewClassId;
                        user.StudentStatus = StudentStatus.Active;
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    await _dbContext.SaveChangesAsync();

                    // Publish UserUpdatedEvent to keep ClassService's CachedUsers and other microservices in sync
                    await _publishEndpoint.Publish(new UserUpdatedEvent
                    {
                        Id = user.Id,
                        UserCode = user.UserCode,
                        FullName = user.FullName,
                        Role = user.Role.ToString(),
                        ClassId = user.ClassId,
                        StudentStatus = user.StudentStatus?.ToString()
                    });
                }
            }
        }
    }
}
