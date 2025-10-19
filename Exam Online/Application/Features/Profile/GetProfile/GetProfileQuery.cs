using MediatR;

namespace Exam_Online_API.Application.Features.Profile.GetProfile
{
    public class GetProfileQuery : IRequest<GetProfileResponse> { }

    public class GetProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
