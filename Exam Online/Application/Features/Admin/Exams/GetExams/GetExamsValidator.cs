using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Exams.GetExams
{
    public class GetExamsValidator : AbstractValidator<GetExamsQuery>
    {
        public GetExamsValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(50).WithMessage("Search term must not exceed 50 characters");

            RuleFor(x => x.StartDateTo)
                .GreaterThanOrEqualTo(x => x.StartDateFrom)
                .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue)
                .WithMessage("Start date 'to' must be after or equal to start date 'from'");

            RuleFor(x => x.EndDateTo)
                .GreaterThanOrEqualTo(x => x.EndDateFrom)
                .When(x => x.EndDateFrom.HasValue && x.EndDateTo.HasValue)
                .WithMessage("End date 'to' must be after or equal to end date 'from'");

            RuleFor(x => x.DurationMax)
                .GreaterThanOrEqualTo(x => x.DurationMin)
                .When(x => x.DurationMin.HasValue && x.DurationMax.HasValue)
                .WithMessage("Maximum duration must be greater than or equal to minimum duration");
        }
    }
}

