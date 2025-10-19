using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Reflection;

namespace Exam_Online.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
               IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<UserExam> UserExams { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditinfo();
            CascadeSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var EntityType in builder.Model.GetEntityTypes())
            {
                if (typeof(IAuditableEntity).IsAssignableFrom(EntityType.ClrType))
                {
                    builder.Entity(EntityType.ClrType).HasQueryFilter(CreateSoftDeleteFilter(EntityType.ClrType));
                }
            }

            base.OnModelCreating(builder);
        }
        private static LambdaExpression CreateSoftDeleteFilter(Type type)
        {
            var parameter = Expression.Parameter(type, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(IAuditableEntity.IsDeleted));
            var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            return Expression.Lambda(condition, parameter);
        }
        private void ApplyAuditinfo()
        {
            var UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
            var TimeNow = DateTime.UtcNow;

            var Entries = ChangeTracker.Entries<IAuditableEntity>();
            foreach (var Entry in Entries)
            {
                var Entity = Entry.Entity;
                switch (Entry.State)
                {
                    case EntityState.Added:
                        Entity.CreatedAt = TimeNow;
                        Entity.CreatedBy = UserName;
                        break;
                    case EntityState.Modified:
                        Entity.UpdatedAt = TimeNow;
                        Entity.UpdatedBy = UserName;
                        break;
                    case EntityState.Deleted:
                        Entry.State = EntityState.Modified;
                        Entity.IsDeleted = true;
                        Entity.DeletedBy = UserName;
                        Entity.DeletedAt = TimeNow;
                        break;
                }
            }
        }
        private void CascadeSoftDelete()
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
            var timeNow = DateTime.UtcNow;

            var deletedEntities = ChangeTracker.Entries<IAuditableEntity>()
                                  .Where(e => e.State == EntityState.Modified && e.Entity.IsDeleted)
                                  .ToList();

            foreach (var entry in deletedEntities)
            {
                var collections = entry.Collections
                    .Where(c => c.CurrentValue != null)
                    .ToList();

                foreach (var collection in collections)
                {
                    var childEntities = collection.CurrentValue!
                        .Cast<object>()
                        .OfType<IAuditableEntity>()
                        .Where(child => !child.IsDeleted)
                        .ToList();

                    foreach (var childEntity in childEntities)
                    {
                        SoftDeleteChildEntity(childEntity, timeNow, userName);
                    }
                }

                var references = entry.References
                                      .Where(r => r.TargetEntry != null && r.TargetEntry.Entity is IAuditableEntity)
                                      .ToList();

                foreach (var reference in references)
                {
                    var childEntity = (IAuditableEntity)reference.TargetEntry!.Entity;

                    if (!childEntity.IsDeleted)
                    {
                        SoftDeleteChildEntity(childEntity, timeNow, userName);
                    }
                }
            }
        }
        private void SoftDeleteChildEntity(IAuditableEntity childEntity, DateTime timeNow, string userName)
        {
            childEntity.IsDeleted = true;
            childEntity.DeletedAt = timeNow;
            childEntity.DeletedBy = userName;
            Entry(childEntity).State = EntityState.Modified;
        }
    }
}
// ملخص الـ Cascade:

/*
CASCADE (سيحذف تلقائياً):
- Category → Exam
- Exam → Question  
- Question → Choice
- Exam → UserExam (اختياري)
- UserExam → UserAnswer

NO ACTION (لن يحذف تلقائياً):
- User → UserExam (حفظ تاريخ المستخدمين)
- Question → UserAnswer (حفظ إجابات الطلاب للتاريخ)
- Choice → UserAnswer (حفظ إجابات الطلاب للتاريخ)

سيناريوهات الحذف:

1. حذف Category:
   Category → Exams → Questions → Choices
   Category → Exams → UserExams → UserAnswers

2. حذف Exam:
   Exam → Questions → Choices
   Exam → UserExams → UserAnswers

3. حذف Question:
   Question → Choices فقط
   UserAnswers تبقى موجودة للتاريخ

4. حذف User:
   لا يحذف UserExams (تبقى للتاريخ)
   
5. حذف UserExam:
   UserExam → UserAnswers
*/
