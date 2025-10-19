using Exam_Online;
using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Behaviors;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Features.Authentication.ForgotPassword;
using Exam_Online_API.Domain.Entities;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace Exam_Online_API.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            services.AddMediatR(MediatR => MediatR.RegisterServicesFromAssemblies(typeof(Program).Assembly));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            var connectionString = configuration.GetConnectionString("SqlConnection");


            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString,
                                         new SqlServerStorageOptions
                                         {
                                             CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                             SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                             QueuePollInterval = TimeSpan.Zero,
                                             UseRecommendedIsolationLevel = true,
                                             DisableGlobalLocks = true,
                                             SchemaName = "hangfire"
                                         })
                    );

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 10; 
                options.Queues = new[] { "default", "file-operations" }; 
                options.ServerName = $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}";
            });

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            services.AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
              {
                 options.SaveToken = true;
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = configuration["Jwt:Issuer"],
                     ValidAudience = configuration["Jwt:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                 };
              }).AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                });

            services.Configure<BaseUrl>(configuration.GetSection("BaseUrl"));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
                {
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromSeconds(10);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 10;
                });
            });


            return services;

        }
    }
}

