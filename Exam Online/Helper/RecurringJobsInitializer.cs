using Exam_Online_API.Infrastructure.Services.BackgroundJobs.ExamStatusUpdat;
using Hangfire;

namespace Exam_Online_API.Helper
{
    public class RecurringJobsInitializer : IHostedService
    {
        private readonly IRecurringJobManager _jobManager;

        public RecurringJobsInitializer(IRecurringJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _jobManager.AddOrUpdate<IExamStatusUpdateJob>(
                "update-expired-exams",
                job => job.UpdateExpiredExamsAsync(),
                "0 * * * *"
            );

            _jobManager.AddOrUpdate<IExamStatusUpdateJob>(
                "activate-scheduled-exams",
                job => job.ActivateScheduledExamsAsync(),
                "*/5 * * * *"
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
