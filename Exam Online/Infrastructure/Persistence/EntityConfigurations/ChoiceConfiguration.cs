using Exam_Online_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class ChoiceConfiguration : BaseEntityConfiguration<Choice>
    {
        public override void Configure(EntityTypeBuilder<Choice> builder)
        {
            base.Configure(builder);
            builder.Property(Choice => Choice.TextChoice).HasMaxLength(100).IsRequired();

            builder.HasMany(Choice => Choice.userAnswers)
                   .WithOne(UserAnswer => UserAnswer.Choice)
                   .HasForeignKey(UserAnswer => UserAnswer.ChoiceId)
                   .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
