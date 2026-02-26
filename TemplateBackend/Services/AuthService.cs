using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TemplateBackend.Data;
using TemplateBackend.DTO;
using TemplateBackend.Models;
using TemplateBackend.Settings;

namespace TemplateBackend.Services
{
    public class AuthService
    {
        private readonly TaskDbContext _db;
        private readonly JwtSettings _jwt;

        public AuthService(TaskDbContext db, IOptions<JwtSettings> jwt)
        {
            _db = db;
            _jwt = jwt.Value;
        }

        // ── REGISTER ──────────────────────────────────────
        public async Task<(bool Ok, string Message, AuthResponseDTO? Data)> RegisterAsync(RegisterDTO dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower().Trim()))
                return (false, "Email already registered!", null);

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username.Trim()))
                return (false, "Username already taken!", null);

            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.ToLower().Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return (true, "Registered successfully!", GenerateToken(user));
        }

        // ── LOGIN ─────────────────────────────────────────
        public async Task<(bool Ok, string Message, AuthResponseDTO? Data)> LoginAsync(LoginDTO dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

            if (user == null || !user.IsActive)
                return (false, "Invalid email or password!", null);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return (false, "Invalid email or password!", null);

            return (true, "Login successful!", GenerateToken(user));
        }

        // ── GET PROFILE ───────────────────────────────────
        public async Task<UserResponseDTO?> GetProfileAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        // ── CHANGE PASSWORD ───────────────────────────────
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password)) return false;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── GENERATE JWT ──────────────────────────────────
        private AuthResponseDTO GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(_jwt.ExpiryInDays);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Role,           user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiry
            };
        }
    }
}
