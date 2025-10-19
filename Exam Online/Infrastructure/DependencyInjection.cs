using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Infrastructure.Services.BackgroundJobs.ExamStatusUpdat;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.EmailService;
using Exam_Online_API.Infrastructure.Services.FileService;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Infrastructure.Services.TokenBlacklistService;
using Exam_Online_API.Infrastructure.Services.TokenService;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Exam_Online_API.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(sql =>
            {
                sql.UseSqlServer(configuration.GetConnectionString("SqlConnection"));
            });

            services.AddScoped<IFileService, FileService>();

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            });
            services.AddScoped<IExamStatusUpdateJob, ExamStatusUpdateJob>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();

            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));


            services.AddMemoryCache();

            services.AddSingleton<IConnectionMultiplexer>(Options =>
            {
                var ConnectionString = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(ConnectionString!);
            });

            services.AddSingleton<IHybridCacheService, HybridCacheService>();

            services.AddSingleton<ICacheInvalidationService, CacheInvalidationService>();

            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();




            return services;

        }
    }
}
