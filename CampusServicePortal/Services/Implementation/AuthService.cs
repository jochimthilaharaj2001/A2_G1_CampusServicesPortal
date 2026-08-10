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
        private readonly IStudentMasterListRepository _masterListRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IStudentMasterListRepository masterListRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailService emailService,
            IFacultyRepository facultyRepository,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _masterListRepository = masterListRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
            _facultyRepository = facultyRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var masterRecord =
                await _masterListRepository.GetByIndexNumberAsync(dto.IndexNumber);

            if (masterRecord == null)
            {
                throw new InvalidOperationException(
                    $"Index number '{dto.IndexNumber}' was not found in the university master list.");
            }

            if (masterRecord.IsRegistered)
            {
                throw new InvalidOperationException(
                    $"Index number '{dto.IndexNumber}' is already linked to an existing account.");
            }

            var existingUser =
                await _authRepository.GetUserByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "An account with this email address already exists.");
            }

            var verificationToken =
                Guid.NewGuid().ToString("N") +
                Guid.NewGuid().ToString("N");

            var verificationExpiry = DateTime.UtcNow.AddHours(
                double.Parse(
                    _configuration["TokenSettings:EmailVerificationExpiryHours"]
                    ?? "24"));

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2,
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = verificationExpiry,
                CreatedDate = DateTime.UtcNow
            };

            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();

            var faculty = await ResolveFacultyAsync(masterRecord.Faculty);

            var student = new Student
            {
                UserId = user.UserId,
                IndexNumber = dto.IndexNumber,
                FacultyId = faculty?.FacultyId,
                Faculty = faculty?.Name ?? masterRecord.Faculty,
                DegreeProgram = dto.DegreeProgram,
                EnrollmentYear = dto.EnrollmentYear,
                ContactNumber = dto.PhoneNumber,
                Address = dto.Address,
                IsActive = true
            };

            await _authRepository.AddStudentAsync(student);
            await _masterListRepository.MarkAsRegisteredAsync(dto.IndexNumber);
            await _authRepository.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(
                user.Email,
                user.FullName,
                verificationToken);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                StudentId = student.StudentId,
                FullName = user.FullName,
                Email = user.Email,
                Role = "Student",
                Token = null,
                Expiration = null,
                Message = "Registration successful. Please verify your email before logging in."
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            User? user;

            if (dto.Role == "Admin")
            {
                if (string.IsNullOrWhiteSpace(dto.Username))
                {
                    throw new UnauthorizedAccessException(
                        "Username is required for admin login.");
                }

                user = await _authRepository
                    .GetUserByUsernameAsync(dto.Username);

                if (user == null || user.Role?.RoleName != "Admin")
                {
                    throw new UnauthorizedAccessException(
                        "Invalid admin username or password.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    throw new UnauthorizedAccessException(
                        "Email is required for student login.");
                }

                user = await _authRepository
                    .GetUserByEmailAsync(dto.Email);

                if (user == null || user.Role?.RoleName != "Student")
                {
                    throw new UnauthorizedAccessException(
                        "Invalid email or password.");
                }
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

            if (!passwordValid)
            {
                return null;
            }

            if (!user.EmailVerified)
            {
                throw new UnauthorizedAccessException(
                    "Your account has not been verified. Please check your email.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Your account has been deactivated. Please contact administration.");
            }

            return new AuthResponseDto
            {
                UserId = user.UserId,
                StudentId = user.Student?.StudentId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "Student",
                Token = GenerateJwtToken(user),
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Message = "Login successful."
            };
        }

        public async Task VerifyEmailAsync(string token)
        {
            var user =
                await _authRepository.GetUserByVerificationTokenAsync(token);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "The verification link is invalid or has already been used.");
            }

            if (!user.EmailVerificationTokenExpiresAt.HasValue ||
                user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "The verification link has expired.");
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return;
            }

            if (user.EmailVerified)
            {
                throw new InvalidOperationException(
                    "This account has already been verified.");
            }

            var newToken =
                Guid.NewGuid().ToString("N") +
                Guid.NewGuid().ToString("N");

            var expiry = DateTime.UtcNow.AddHours(
                double.Parse(
                    _configuration["TokenSettings:EmailVerificationExpiryHours"]
                    ?? "24"));

            user.EmailVerificationToken = newToken;
            user.EmailVerificationTokenExpiresAt = expiry;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(
                user.Email,
                user.FullName,
                newToken);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return;
            }

            await _passwordResetTokenRepository
                .InvalidatePreviousTokensAsync(user.UserId);

            var expiryMinutes = double.Parse(
                _configuration["TokenSettings:PasswordResetExpiryMinutes"]
                ?? "30");

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = Guid.NewGuid().ToString("N") +
                        Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetTokenRepository.CreateTokenAsync(resetToken);
            await _passwordResetTokenRepository.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.FullName,
                resetToken.Token);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var tokenRecord =
                await _passwordResetTokenRepository
                    .GetValidTokenAsync(dto.Token);

            if (tokenRecord == null)
            {
                throw new InvalidOperationException(
                    "The password reset link is invalid or expired.");
            }

            var user = tokenRecord.User ??
                       await _authRepository
                           .GetUserByIdAsync(tokenRecord.UserId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _passwordResetTokenRepository
                .MarkTokenUsedAsync(tokenRecord.TokenId);

            await _passwordResetTokenRepository
                .InvalidatePreviousTokensAsync(user.UserId);

            foreach (var refreshToken in user.RefreshTokens)
            {
                refreshToken.IsRevoked = true;
            }

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();
            await _passwordResetTokenRepository.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JWT Key is missing in appsettings.json.");
            }

            var issuer =
                _configuration["Jwt:Issuer"] ?? "CampusServicePortal";

            var audience =
                _configuration["Jwt:Audience"]
                ?? "CampusServicePortalUsers";

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
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
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new Claim(
                    ClaimTypes.Role,
                    user.Role?.RoleName ?? "Student"),

                new Claim(
                    "studentId",
                    user.Student?.StudentId.ToString() ?? "0")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        private async Task<Faculty?> ResolveFacultyAsync(
            string? facultyName)
        {
            if (string.IsNullOrWhiteSpace(facultyName))
            {
                return null;
            }

            var existing =
                await _facultyRepository.GetByNameAsync(
                    facultyName.Trim());

            if (existing != null)
            {
                return existing;
            }

            var faculty = new Faculty
            {
                Name = facultyName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _facultyRepository.AddAsync(faculty);
            await _facultyRepository.SaveChangesAsync();

            return faculty;
        }
    }
}