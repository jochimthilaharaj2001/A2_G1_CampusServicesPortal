using CampusServicePortal.Data;
using CampusServicePortal.Helpers;
using CampusServicePortal.Repositories.Implementation;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Implementation;
using CampusServicePortal.Services.Interfaces;
using CampusServicesPortal.Hostel.Repositories;
using CampusServicesPortal.Hostel.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CampusServicePortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Keep validation settings aligned with AuthService.GenerateJwtToken.
            // Local development may supply only Jwt:Key through user secrets.
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CampusServicePortal";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CampusServicePortalUsers";
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.");

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Campus Services Portal API",
                    Version = "v1",
                    Description = "REST API for University Campus Services Portal"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT token as: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            builder.Services.AddAuthorization();

            // Helpers
            builder.Services.AddScoped<JwtHelper>();

            // Repositories
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IStudentMasterListRepository, StudentMasterListRepository>();
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
            builder.Services.AddScoped<ICertificateTypeRepository, CertificateTypeRepository>();
            builder.Services.AddScoped<ILabReservationRepository, LabReservationRepository>();

            builder.Services.AddScoped<IHostelRepository, HostelRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IHostelApplicationRepository, HostelApplicationRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Authentication and student services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IStudentMasterListService, StudentMasterListService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IFacultyService, FacultyService>();
            builder.Services.AddScoped<ICertificateTypeService, CertificateTypeService>();
            builder.Services.AddScoped<ILabReservationService, LabReservationService>();
            builder.Services.AddScoped<ILabService, LabService>();

            // Hostel and notification services
            builder.Services.AddScoped<IHostelService, HostelService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IHostelApplicationService, HostelApplicationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationQueue, NotificationQueue>();

            // Other modules
            builder.Services.AddScoped<IComplaintService, ComplaintService>();
            builder.Services.AddScoped<IFeePaymentService, FeePaymentService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<ICertificateRequestService, CertificateRequestService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Database.Migrate();
                DatabaseSeeder.Seed(db);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
