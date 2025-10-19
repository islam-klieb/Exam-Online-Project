using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class QuestionConfiguration : BaseEntityConfiguration<Question>
    {
        public override void Configure(EntityTypeBuilder<Question> builder)
        {
            base.Configure(builder);
            builder.Property(Question => Question.Title).HasMaxLength(100).IsRequired();
            builder.Property(q => q.Type)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(QuestionType.SingleChoice);

            builder.HasMany(Question => Question.Choices)
                   .WithOne(Choice => Choice.Question)
                   .HasForeignKey(Choice => Choice.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(Question => Question.userAnswers)
                   .WithOne(UserAnswer => UserAnswer.Question)
                   .HasForeignKey(UserAnswer => UserAnswer.QuestionId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
