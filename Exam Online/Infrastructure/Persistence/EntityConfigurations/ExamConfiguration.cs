using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class ExamConfiguration : BaseEntityConfiguration<Exam>
    {
        public override void Configure(EntityTypeBuilder<Exam> builder)
        {
            base.Configure(builder);
            builder.Property(Exam => Exam.Title).HasMaxLength(20).IsRequired();
            builder.Property(Exam => Exam.Duration).IsRequired().HasComment("Duration in minutes, min: 20, max: 180");
            builder.Property(Exam => Exam.Icon).IsRequired();
            builder.Property(Exam => Exam.StartDate).IsRequired();
            builder.Property(Exam => Exam.EndDate).IsRequired();
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(ExamStatus.Draft);

            builder.HasMany(Exam => Exam.Questions)
                   .WithOne(Question => Question.Exam)
                   .HasForeignKey(Question => Question.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(Exam => Exam.UserExams)
                   .WithOne(UserExams => UserExams.Exam)
                   .HasForeignKey(UserExams => UserExams.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
