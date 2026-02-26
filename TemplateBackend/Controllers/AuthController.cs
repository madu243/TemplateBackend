using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TemplateBackend.DTO;
using TemplateBackend.Services;

namespace TemplateBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) => _auth = auth;

        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (ok, message, data) = await _auth.RegisterAsync(dto);
            if (!ok) return Conflict(new { message });
            return Ok(new { message, data });
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (ok, message, data) = await _auth.LoginAsync(dto);
            if (!ok) return Unauthorized(new { message });
            return Ok(new { message, data });
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var data = await _auth.GetProfileAsync(userId);
            if (data == null) return NotFound(new { message = "User not found!" });
            return Ok(new { message = "Profile retrieved!", data });
        }

        // POST api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ok = await _auth.ChangePasswordAsync(userId, dto);
            if (!ok) return BadRequest(new { message = "Current password is incorrect!" });
            return Ok(new { message = "Password changed successfully!" });
        }
    }
}







