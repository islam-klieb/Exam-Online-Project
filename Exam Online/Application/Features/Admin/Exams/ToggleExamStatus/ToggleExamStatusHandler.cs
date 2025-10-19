using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Exam_Online_API.Application.Features.Admin.Exams.ToggleExamStatus
{
    public class ToggleExamStatusHandler : IRequestHandler<ToggleExamStatusCommand, ToggleExamStatusResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ToggleExamStatusHandler> _logger;
        private readonly ICacheInvalidationService _cache;

        public ToggleExamStatusHandler(
            ApplicationDbContext context,
            ILogger<ToggleExamStatusHandler> logger,
            ICacheInvalidationService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ToggleExamStatusResponse> Handle(
            ToggleExamStatusCommand request,
            CancellationToken cancellationToken)
        {
            

            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with ID {request.Id} not found");
            }

            try
            {
                var now = DateTime.UtcNow;
                var previousIsActive = exam.IsActive;
                var previousStatus = exam.Status;

                exam.IsActive = !exam.IsActive;

                string message;

                if (exam.IsActive)
                {
                    if (exam.EndDate < now)
                    {
                        exam.IsActive = false;
                        throw new BusinessLogicException(
                            "Cannot activate an exam that has already ended. Please update the exam dates first.");
                    }

                    if (exam.StartDate > now)
                    {
                        exam.Status = ExamStatus.Scheduled;
                        message = $"Exam '{exam.Title}' activated and scheduled to start on {exam.StartDate:yyyy-MM-dd HH:mm}";
                    }
                    else if (exam.StartDate <= now && exam.EndDate >= now)
                    {
                        exam.Status = ExamStatus.Active;
                        message = $"Exam '{exam.Title}' is now active and available to users";
                    }
                    else
                    {
                        exam.Status = ExamStatus.Draft;
                        message = $"Exam '{exam.Title}' activated";
                    }
                }
                else
                {
                    exam.Status = ExamStatus.Draft;
                    message = $"Exam '{exam.Title}' has been deactivated and is no longer available to users";
                }

                await _context.SaveChangesAsync(cancellationToken);
                await _cache.InvalidateExamCacheAsync();
                _logger.LogInformation(
                    "Exam status toggled. ID: {ExamId}, Title: {Title}, " +
                    "Previous IsActive: {PreviousIsActive}, New IsActive: {NewIsActive}, " +
                    "Previous Status: {PreviousStatus}, New Status: {NewStatus}",
                    exam.Id,
                    exam.Title,
                    previousIsActive,
                    exam.IsActive,
                    previousStatus,
                    exam.Status);

                return new ToggleExamStatusResponse
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    IsActive = exam.IsActive,
                    Status = exam.Status,
                    Message = message
                };
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling exam status for ID: {ExamId}", request.Id);
                throw new BusinessLogicException($"Failed to toggle exam status: {ex.Message}");
            }
        }
    }
}
