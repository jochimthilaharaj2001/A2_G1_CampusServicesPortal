using CampusServicePortal.Data;
using CampusServicePortal.Helpers;
using CampusServicePortal.Repositories.Implementation;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Implementation;
using CampusServicePortal.Services.Interfaces;
using CampusServicesPortal.Hostel.Repositories;
using CampusServicesPortal.Hostel.Services;
using Microsoft.EntityFrameworkCore;

namespace CampusServicePortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<JwtHelper>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IHostelService, HostelService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddScoped<IHostelApplicationService, HostelApplicationService>();

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IHostelRepository, HostelRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IHostelApplicationRepository, HostelApplicationRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
