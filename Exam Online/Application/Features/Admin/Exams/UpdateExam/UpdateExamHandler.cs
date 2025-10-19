using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Exams.UpdateExam
{
    public class UpdateExamHandler : IRequestHandler<UpdateExamCommand, UpdateExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<UpdateExamHandler> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICacheInvalidationService _cache;

        public UpdateExamHandler(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<UpdateExamHandler> logger,
            ICacheInvalidationService cache,
            IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
        }

        public async Task<UpdateExamResponse> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
        {

            var exam = await _context.Exams
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with ID {request.Id} not found");
            }

            try
            {
                if (exam.CategoryId != request.CategoryId)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

                    if (!categoryExists)
                    {
                        throw new NotFoundException($"Category with ID {request.CategoryId} not found");
                    }
                }

                if (exam.Title.ToLower() != request.Title.Trim().ToLower())
                {
                    var titleExists = await _context.Exams
                        .AnyAsync(e => e.Id != request.Id &&
                                      e.CategoryId == request.CategoryId &&
                                      e.Title.ToLower() == request.Title.Trim().ToLower(), cancellationToken);

                    if (titleExists)
                    {
                        throw new ConflictException("An exam with this title already exists in the selected category");
                    }
                }

                var oldIconPath = exam.Icon;
                var iconChanged = false;

                exam.Title = request.Title.Trim();
                exam.CategoryId = request.CategoryId;
                exam.StartDate = request.StartDate;
                exam.EndDate = request.EndDate;
                exam.Duration = request.Duration;

                if (request.Icon != null)
                {
                    var newIconPath = await _fileService.UploadFileAsync(request.Icon,"exams",
                        cancellationToken);

                    exam.Icon = newIconPath;
                    iconChanged = true;


                    var jobId = _backgroundJobClient.Enqueue<IFileService>(service => service.DeleteFileAsync(oldIconPath, CancellationToken.None));

                    _logger.LogInformation(
                        "Enqueued file deletion job. JobId: {JobId}, File: {FilePath}, Exam: {ExamId}",
                        jobId,oldIconPath,exam.Id);
                }

                var now = DateTime.UtcNow;
                ExamStatus newStatus;

                if (request.StartDate > now)
                {
                    newStatus = ExamStatus.Scheduled;
                }
                else if (request.StartDate <= now && request.EndDate >= now)
                {
                    newStatus = ExamStatus.Active;
                }
                else if (request.EndDate < now)
                {
                    newStatus = ExamStatus.Completed;
                    exam.IsActive = false;
                }
                else
                {
                    newStatus = exam.Status;
                }

                var statusChanged = exam.Status != newStatus;
                exam.Status = newStatus;

                await _context.SaveChangesAsync(cancellationToken);

                await _cache.InvalidateExamCacheAsync();

                await _context.Entry(exam).Reference(e => e.Category).LoadAsync(cancellationToken);

                _logger.LogInformation(
                    "Exam updated successfully. ID: {ExamId}, Title: {Title}, Icon Changed: {IconChanged}, Status Changed: {StatusChanged} (Old: {OldStatus}, New: {NewStatus})",
                    exam.Id,
                    exam.Title,
                    iconChanged,
                    statusChanged,
                    statusChanged ? exam.Status.ToString() : "N/A",
                    newStatus);

                return new UpdateExamResponse
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    IconPath = exam.Icon,
                    CategoryId = exam.CategoryId,
                    CategoryTitle = exam.Category.Title,
                    StartDate = exam.StartDate,
                    EndDate = exam.EndDate,
                    Duration = exam.Duration,
                    Status = exam.Status,
                    UpdatedAt = exam.UpdatedAt
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ConflictException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam with ID: {ExamId}", request.Id);
                throw new BusinessLogicException($"Failed to update exam: {ex.Message}");
            }
        }
    }
}
