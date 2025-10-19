using Exam_Online_API.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam_Online.Infrastructure.Persistence.EntityConfigurations
{
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(Entity => Entity.Id);
            builder.Property(Entity => Entity.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(Entity => Entity.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(Entity => Entity.IsDeleted).HasDefaultValue(false);
        }
    }
}
