using Exam_Online_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(User => User.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(User => User.IsDeleted).HasDefaultValue(false);
            builder.Property(User => User.FirstName).HasMaxLength(20).IsRequired();
            builder.Property(User => User.LastName).HasMaxLength(20).IsRequired();
            builder.Property(User => User.Email).IsRequired();
            builder.Property(User => User.PhoneNumber).IsRequired();


            builder.HasMany(User => User.UserExams)
                   .WithOne(UserExam => UserExam.User)
                   .HasForeignKey(UserExam => UserExam.UserId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
