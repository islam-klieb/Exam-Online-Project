using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Shared.Pagination
{
    public static class Pagination<T> where T : BaseEntity
    {
        public static IQueryable<T> OffsetPagination(IQueryable<T> query, int PageNumber ,int PageSize)
        {
            query = query
                       .Skip((PageNumber - 1) * PageSize)
                       .Take(PageSize);
           return query;
        }
        public static IQueryable<T> KeysetPagination(IQueryable<T> query, DateTime? lastCreatedAt , SortDirection sort , int PageSize)
        {
            if (lastCreatedAt.HasValue)
            {
                if (sort == SortDirection.Ascending)
                    query = query.Where(q => q.CreatedAt > lastCreatedAt.Value);
                else
                    query = query.Where(q => q.CreatedAt < lastCreatedAt.Value);
            }
            query = query.Take(PageSize + 1);
            return query;
        }
        public static IQueryable<T> KeysetPagination(IQueryable<T> query, DateTime? lastCreatedAt, int PageSize)
        {
            if (lastCreatedAt.HasValue)
                query = query.Where(q => q.CreatedAt < lastCreatedAt.Value);
            
            query = query.Take(PageSize + 1);
            return query;
        }
    }
}
