using Exam_Online_API.Application;
using Exam_Online_API.Application.Extensions;
using Exam_Online_API.Helper;
using Exam_Online_API.Infrastructure;
using Exam_Online_API.Middlewares.TokenBlacklistMiddleware;
using Hangfire;
using Scalar.AspNetCore;

namespace Exam_Online
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Elevate Online Exam API",
                    Version = "v1",
                    Description = "API for online examination system with admin and user features",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Exam Online Team",
                        Email = "support@examonline.com"
                    }
                });
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.CustomSchemaIds(type => type.FullName);

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {   Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddHostedService<RecurringJobsInitializer>();

            var app = builder.Build();
            await DbInitializer.ApplyMigrationsAsync(app);
            await DbInitializer.SeedRoles(app);
            await DbInitializer.SeedUserAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapScalarApiReference(options =>
                {
                    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
                });

            }

            app.UseHttpsRedirection();

            app.UseMiddleware<TokenBlacklistMiddleware>();

            app.UseExceptionHandler();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                DashboardTitle = "Exam Online - Background Jobs",
                StatsPollingInterval = 2000, 
                DisplayStorageConnectionString = false
            });

            app.MapControllers();


            app.Run();
        }
    }
}
