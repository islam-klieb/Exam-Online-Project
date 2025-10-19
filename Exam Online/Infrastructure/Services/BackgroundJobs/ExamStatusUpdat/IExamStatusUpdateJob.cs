namespace Exam_Online_API.Infrastructure.Services.BackgroundJobs.ExamStatusUpdat
{
    public interface IExamStatusUpdateJob
    {
        Task UpdateExpiredExamsAsync();
        Task ActivateScheduledExamsAsync();
    }
}
