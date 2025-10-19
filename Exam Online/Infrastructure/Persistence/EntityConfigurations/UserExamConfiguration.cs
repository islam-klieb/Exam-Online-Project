using Exam_Online_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class UserExamConfiguration : BaseEntityConfiguration<UserExam>
    {
        public override void Configure(EntityTypeBuilder<UserExam> builder)
        {
            base.Configure(builder);
            builder.Property(UserExam => UserExam.AttemptDate).IsRequired();
            builder.Property(UserExam => UserExam.Score).IsRequired();
            builder.Property(UserExam => UserExam.DurationTaken).IsRequired();



            builder.HasMany(UserExam => UserExam.userAnswers)
                   .WithOne(UserAnswer => UserAnswer.UserExam)
                   .HasForeignKey(UserAnswer => UserAnswer.UserExamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
