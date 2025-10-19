using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Exam_Online_API.Controllers
{
    [ApiController]
    [EnableRateLimiting("api")]
    public class ApiBaseController : ControllerBase
    {
    }
}
