using CampusServicePortal.DTOs.Auth;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CampusServicePortal.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;


        public AuthService(
            IAuthRepository authRepository,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }



        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser =
                await _authRepository.GetUserByEmailAsync(dto.Email);


            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }


            var user = new User
            {
                FullName = dto.FullName,

                Email = dto.Email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Password),

                RoleId = 2
            };


            await _authRepository.AddUserAsync(user);

            await _authRepository.SaveChangesAsync();



            var student = new Student
            {
                UserId = user.UserId,

                StudentNumber = dto.StudentNumber,

                Faculty = dto.Faculty,

                DegreeProgram = dto.DegreeProgram,

                EnrollmentYear = dto.EnrollmentYear
            };


            await _authRepository.AddStudentAsync(student);

            await _authRepository.SaveChangesAsync();



            return new AuthResponseDto
            {
                UserId = user.UserId,

                FullName = user.FullName,

                Email = user.Email,

                Role = "Student",

                Token = GenerateToken(user),

                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }





        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user =
                await _authRepository.GetUserByEmailAsync(dto.Email);



            if (user == null)
            {
                return null;
            }



            bool passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash);



            if (!passwordValid)
            {
                return null;
            }



            return new AuthResponseDto
            {
                UserId = user.UserId,

                FullName = user.FullName,

                Email = user.Email,

                Role = user.Role?.RoleName ?? "Student",

                Token = GenerateToken(user),

                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }





        private string GenerateToken(User user)
        {

            var jwtKey = _configuration["Jwt:Key"];


            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception(
                    "JWT Key is missing in appsettings.json");
            }



            var issuer =
                _configuration["Jwt:Issuer"]
                ?? "CampusServicePortal";



            var audience =
                _configuration["Jwt:Audience"]
                ?? "CampusServicePortalUsers";



            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey));



            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);



            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.UserId.ToString()),


                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email),


                new Claim(
                    ClaimTypes.Role,
                    user.Role?.RoleName ?? "Student")
            };



            var token =
                new JwtSecurityToken(

                    issuer: issuer,

                    audience: audience,

                    claims: claims,

                    expires:
                    DateTime.UtcNow.AddMinutes(60),

                    signingCredentials: credentials
                );



            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}