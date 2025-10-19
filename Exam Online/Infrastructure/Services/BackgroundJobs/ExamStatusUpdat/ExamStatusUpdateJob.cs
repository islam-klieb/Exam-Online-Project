using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Infrastructure.Services.BackgroundJobs.ExamStatusUpdat
{
    public class ExamStatusUpdateJob : IExamStatusUpdateJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamStatusUpdateJob> _logger;

        public ExamStatusUpdateJob(ApplicationDbContext context, ILogger<ExamStatusUpdateJob> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task UpdateExpiredExamsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredExams = await _context.Exams
                    .Where(e => e.IsActive &&
                                e.Status == ExamStatus.Active &&
                                e.EndDate < now)
                    .ToListAsync();

                foreach (var exam in expiredExams)
                {
                    exam.Status = ExamStatus.Completed;
                    exam.IsActive = false;
                }

                if (expiredExams.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated {Count} expired exams to Completed status", expiredExams.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expired exams");
                throw;
            }
        }

        public async Task ActivateScheduledExamsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var scheduledExams = await _context.Exams
                    .Where(e => e.Status == ExamStatus.Scheduled &&
                                e.StartDate <= now &&
                                e.EndDate >= now)
                    .ToListAsync();

                foreach (var exam in scheduledExams)
                {
                    exam.Status = ExamStatus.Active;
                }

                if (scheduledExams.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Activated {Count} scheduled exams", scheduledExams.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating scheduled exams");
                throw;
            }
        }
    }
}
