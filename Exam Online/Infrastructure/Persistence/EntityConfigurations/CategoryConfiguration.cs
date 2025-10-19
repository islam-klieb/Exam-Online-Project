using Exam_Online_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class CategoryConfiguration : BaseEntityConfiguration<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            base.Configure(builder);
            builder.Property(User => User.Title).HasMaxLength(20).IsRequired();
            builder.Property(User => User.Icon).IsRequired();


            builder.HasMany(Category => Category.Exams)
                   .WithOne(Exam => Exam.Category)
                   .HasForeignKey(Exam => Exam.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
