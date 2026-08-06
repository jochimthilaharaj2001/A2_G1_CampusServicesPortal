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
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IStudentMasterListRepository masterListRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _masterListRepository = masterListRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        // ── Register ─────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Validate index number against StudentMasterList (BRD Rule #5)
            var masterRecord = await _masterListRepository.GetByIndexNumberAsync(dto.IndexNumber);
            if (masterRecord == null)
                throw new InvalidOperationException(
                    $"Index number '{dto.IndexNumber}' was not found in the university master list. " +
                    "Please contact the Registrar's office.");

            if (masterRecord.IsRegistered)
                throw new InvalidOperationException(
                    $"Index number '{dto.IndexNumber}' is already linked to an existing account.");

            // 2. Check email uniqueness
            var existingUser = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("An account with this email address already exists.");

            // 3. Generate email verification token (BRD Rule #16)
            var verificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var verificationExpiry = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["TokenSettings:EmailVerificationExpiryHours"] ?? "24"));

            // 4. Create User record — EmailVerified = false until token is confirmed
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2, // Student
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = verificationExpiry,
                CreatedDate = DateTime.UtcNow
            };

            await _authRepository.AddUserAsync(user);
            await _authRepository.SaveChangesAsync();

            // 5. Create Student record — Faculty pre-filled from master list
            var student = new Student
            {
                UserId = user.UserId,
                IndexNumber = dto.IndexNumber,
                Faculty = masterRecord.Faculty,
                DegreeProgram = dto.DegreeProgram,
                EnrollmentYear = dto.EnrollmentYear,
                ContactNumber = dto.PhoneNumber,
                Address = dto.Address,
                IsActive = true
            };

            await _authRepository.AddStudentAsync(student);

            // 6. Mark master list record as registered (prevents duplicate accounts)
            await _masterListRepository.MarkAsRegisteredAsync(dto.IndexNumber);
            await _authRepository.SaveChangesAsync();

            // 7. Send verification email
            try
            {
                await _emailService.SendVerificationEmailAsync(
                    user.Email, user.FullName, verificationToken);
            }
            catch (Exception)
            {
                // Registration still succeeds even if email fails.
                // Student can request a resend via /api/auth/resend-verification.
            }

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = "Student",
                Token = null, // No JWT until email is verified
                Expiration = null,
                Message = "Registration successful. Please check your email to verify your account before logging in."
            };

        }

        // ── Login ─────────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (user == null) return null;

            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid) return null;

            // BRD Rule #16: Block login if email not verified (distinct error)
            if (!user.EmailVerified)
                throw new UnauthorizedAccessException(
                    "Your account has not been verified yet. " +
                    "Please check your email for the verification link, " +
                    "or request a new one at /api/auth/resend-verification.");

            // BRD Module 1.4: Block login if account deactivated (distinct error)
            if (!user.IsActive)
                throw new UnauthorizedAccessException(
                    "Your account has been deactivated. " +
                    "Please contact the university administration for assistance.");

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "Student",
                Token = GenerateJwtToken(user),
                Expiration = DateTime.UtcNow.AddMinutes(60),
                Message = "Login successful."
            };
        }

        // ── Email Verification ────────────────────────────────────────────────────

        public async Task VerifyEmailAsync(string token)
        {
            // Find user with matching verification token
            var user = await _authRepository.GetUserByVerificationTokenAsync(token);

            if (user == null)
                throw new InvalidOperationException(
                    "The verification link is invalid or has already been used.");

            if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "The verification link has expired. " +
                    "Please request a new one at /api/auth/resend-verification.");

            // Mark email as verified and clear the token
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            // Silently return if no account found — prevents account enumeration
            if (user == null) return;

            if (user.EmailVerified)
                throw new InvalidOperationException("This account has already been verified.");

            // Invalidate old token and generate a new one (BRD Rule: new token invalidates old)
            var newToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["TokenSettings:EmailVerificationExpiryHours"] ?? "24"));

            user.EmailVerificationToken = newToken;
            user.EmailVerificationTokenExpiresAt = expiry;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, newToken);
        }

        // ── Forgot / Reset Password ───────────────────────────────────────────────

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            // BRD Rule: Response NEVER reveals if the email exists (prevents account enumeration)
            if (user == null) return;

            // Invalidate any existing unused tokens for this user
            await _passwordResetTokenRepository.InvalidatePreviousTokensAsync(user.UserId);

            var expiryMinutes = double.Parse(
                _configuration["TokenSettings:PasswordResetExpiryMinutes"] ?? "30");

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetTokenRepository.CreateTokenAsync(resetToken);
            await _passwordResetTokenRepository.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(
                user.Email, user.FullName, resetToken.Token);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            // BRD Rule: Token must be valid, unused, and not expired
            var tokenRecord = await _passwordResetTokenRepository.GetValidTokenAsync(dto.Token);

            if (tokenRecord == null)
                throw new InvalidOperationException(
                    "The password reset link is invalid, has already been used, or has expired. " +
                    "Please request a new reset link.");

            var user = tokenRecord.User
                ?? await _authRepository.GetUserByIdAsync(tokenRecord.UserId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Update password hash
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Mark the token as used
            await _passwordResetTokenRepository.MarkTokenUsedAsync(tokenRecord.TokenId);

            // Invalidate ALL other active reset tokens for this user (BRD: forces re-login)
            await _passwordResetTokenRepository.InvalidatePreviousTokensAsync(user.UserId);

            // Revoke all refresh tokens to force re-login everywhere
            foreach (var rt in user.RefreshTokens)
            {
                rt.IsRevoked = true;
            }

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.SaveChangesAsync();
            await _passwordResetTokenRepository.SaveChangesAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────


        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing in appsettings.json.");

            var issuer = _configuration["Jwt:Issuer"] ?? "CampusServicePortal";
            var audience = _configuration["Jwt:Audience"] ?? "CampusServicePortalUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Student"),
                new Claim("studentId", user.Student?.StudentId.ToString() ?? "0")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}