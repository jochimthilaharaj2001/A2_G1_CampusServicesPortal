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
                    Description = "Enter JWT Token like: Bearer {your_token}",
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

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // Helpers
            builder.Services.AddScoped<JwtHelper>();

            // ============================================================
            // REPOSITORIES
            // ============================================================

            // Authentication / Users
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Hostel
            builder.Services.AddScoped<IHostelRepository, HostelRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IHostelApplicationRepository, HostelApplicationRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Student
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IStudentMasterListRepository, StudentMasterListRepository>();
            builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();

            // Certificate / Lab / Password
            builder.Services.AddScoped<ICertificateTypeRepository, CertificateTypeRepository>();
            builder.Services.AddScoped<ILabReservationRepository, LabReservationRepository>();
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            // ============================================================
            // SERVICES
            // ============================================================

            // Authentication / Users
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Hostel
            builder.Services.AddScoped<IHostelService, HostelService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IHostelApplicationService, HostelApplicationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Student
            builder.Services.AddScoped<IStudentMasterListService, StudentMasterListService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IFacultyService, FacultyService>();

            // Certificate / Lab / Email
            builder.Services.AddScoped<ICertificateTypeService, CertificateTypeService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ILabReservationService, LabReservationService>();
            builder.Services.AddScoped<ILabService, LabService>();

            var app = builder.Build();

            // ============================================================
            // DATABASE SEEDING
            // ============================================================

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                DatabaseSeeder.Seed(db);
            }

            // ============================================================
            // HTTP PIPELINE
            // ============================================================

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}