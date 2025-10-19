using Exam_Online_API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public class UserAnswerConfiguration : BaseEntityConfiguration<UserAnswer>
    {
        public override void Configure(EntityTypeBuilder<UserAnswer> builder)
        {
            base.Configure(builder);
        }
    }
}
